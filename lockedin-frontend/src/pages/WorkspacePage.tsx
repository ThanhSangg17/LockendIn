// src/pages/WorkspacePage.tsx
import React, { useState, useRef, useEffect } from 'react';
import { Send, Zap, Dumbbell, Plus, Loader2, Trash2, Target, MessageSquare, Paperclip, User } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useData } from '../context/DataContext';
import api from '../services/api';
import { getMockMessages, saveMockMessages } from '../services/mockFirebase';
import { useLocation } from 'react-router-dom';

const WorkspacePage: React.FC = () => {
  const { workspaces, incrementWorkspaceSession, fileDispute, refreshBackendData } = useData();
  const { currentUser } = useAuth();
  const location = useLocation();
  const searchParams = new URLSearchParams(location.search);
  const bookingIdParam = searchParams.get('bookingId');
  const workspaceIdParam = searchParams.get('workspaceId') || searchParams.get('wsId');

  const displayWorkspaces = workspaces.length > 0 ? workspaces : [
    {
      id: 'ws-mock-active',
      bookingId: 'booking-mock-active',
      customerId: currentUser?.role === 'pt' ? 'cust-mock-2' : currentUser?.id || 'cust-me',
      customerName: currentUser?.role === 'pt' ? 'Lê Thị B' : currentUser?.fullName || 'Học Viên',
      ptId: currentUser?.role === 'pt' ? currentUser?.id || 'pt-me' : 'pt-mock-1',
      ptName: currentUser?.role === 'pt' ? currentUser?.fullName || 'PT Test Account' : 'PT Test Account',
      packageName: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
      sessionsTotal: 12,
      sessionsCompleted: 4,
      status: 'active',
      ptNotes: currentUser?.role === 'pt'
        ? 'Học viên thể lực tốt, đẩy ngực đều. Nên tăng thêm bài tập đùi đĩa để phát triển toàn diện.'
        : 'Hãy ăn đủ đạm và giữ chế độ sinh hoạt đều đặn để phục hồi cơ bắp tốt nhất nhé.',
      createdAt: new Date().toISOString()
    }
  ];
  
  const [selectedWorkspace, setSelectedWorkspace] = useState<any>(null);
  const [conversation, setConversation] = useState<any>(null);
  const [messages, setMessages] = useState<any[]>([]);
  const [mealPlansList, setMealPlansList] = useState<any[]>([]);
  
  // Dispute Modal state
  const [showDisputeModal, setShowDisputeModal] = useState(false);
  const [disputeReason, setDisputeReason] = useState('');
  const [disputeDesc, setDisputeDesc] = useState('');
  const [disputeFile, setDisputeFile] = useState<File | null>(null);
  const [submittingDispute, setSubmittingDispute] = useState(false);
  
  const [loadingMessages, setLoadingMessages] = useState(false);
  const [loadingMealPlan, setLoadingMealPlan] = useState(false);
  const [sendingMessage, setSendingMessage] = useState(false);
  const [generatingMealPlan, setGeneratingMealPlan] = useState(false);
  const [useMockChat, setUseMockChat] = useState(false);
  
  const [input, setInput] = useState('');
  const [activeTab, setActiveTab] = useState<'sessions' | 'chat' | 'mealplan'>('chat');
  const [selectedDayIndex, setSelectedDayIndex] = useState(0);
  
  // Create Plan Modal state
  const [showCreatePlanModal, setShowCreatePlanModal] = useState(false);
  const [goal, setGoal] = useState('');
  const [preference, setPreference] = useState('');
  const [allergyNote, setAllergyNote] = useState('');
  const [genError, setGenError] = useState('');
  
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // State for confirmed checkout sessions (to prevent double confirmation)
  const [confirmedSessions, setConfirmedSessions] = useState<string[]>(() => {
    try {
      return JSON.parse(localStorage.getItem('lockedin_confirmed_sessions') || '[]');
    } catch {
      return [];
    }
  });

  // Keep selectedWorkspace in sync with the latest data from displayWorkspaces context
  useEffect(() => {
    if (selectedWorkspace && displayWorkspaces.length > 0) {
      const latest = displayWorkspaces.find(w => w.id === selectedWorkspace.id);
      if (latest && (
        latest.sessionsCompleted !== selectedWorkspace.sessionsCompleted ||
        latest.status !== selectedWorkspace.status ||
        latest.ptNotes !== selectedWorkspace.ptNotes
      )) {
        setSelectedWorkspace(latest);
      }
    }
  }, [displayWorkspaces, selectedWorkspace]);

  // Call the specific REST API endpoint to log a session
  const sendCheckoutRequest = async (wsId: string, sessionNum: number) => {
    setSendingMessage(true);
    try {
      if (!useMockChat) {
        // 1. Log session directly via proper backend endpoint
        const sessionRes = await api.post(`/workspaces/${wsId}/sessions`);
        if (!sessionRes.data?.success) {
          throw new Error('Failed to log session via backend API');
        }
        
        // Refresh context data to sync the new session count
        if (refreshBackendData) refreshBackendData();

        // 2. Post a cosmetic message for UI history (optional)
        const checkoutMessageText = `[CheckoutCompleted:${sessionNum}]`;
        try {
          await api.post(`/conversations/messages`, {
            conversationId: conversation.id,
            content: checkoutMessageText,
            messageType: 'system'
          });
          const msgsRes = await api.get(`/conversations/messages/${conversation.id}`);
          if (msgsRes.data?.success) setMessages(msgsRes.data.data);
        } catch (msgErr) {
          console.warn('Failed to send cosmetic checkout message:', msgErr);
        }
      } else {
        incrementWorkspaceSession(wsId);
        const checkoutMessageText = `[CheckoutCompleted:${sessionNum}]`;
        const newMsg = {
          id: 'msg-' + Date.now(),
          workspaceId: wsId,
          senderId: currentUser?.id || 'user',
          senderName: currentUser?.fullName || 'System',
          text: checkoutMessageText,
          createdAt: new Date().toISOString(),
          read: true
        };
        const allMsgs = getMockMessages();
        allMsgs.push(newMsg);
        saveMockMessages(allMsgs);
        setMessages(prev => [...prev, {
          id: newMsg.id,
          senderUserId: newMsg.senderId,
          content: newMsg.text,
          createdAt: newMsg.createdAt
        }]);
      }
    } catch (e: any) {
      console.error('Failed to log session', e);
      alert('Lỗi: ' + (e.response?.data?.message || 'Không thể hoàn thành buổi tập. Vui lòng thử lại.'));
    } finally {
      setSendingMessage(false);
    }
  };

  const handleConfirmSession = (wsId: string, sessionNum: number) => {
    const key = `${wsId}-${sessionNum}`;
    if (!confirmedSessions.includes(key)) {
      const newConfirmed = [...confirmedSessions, key];
      setConfirmedSessions(newConfirmed);
      localStorage.setItem('lockedin_confirmed_sessions', JSON.stringify(newConfirmed));
      
      // Update backend / local stats
      incrementWorkspaceSession(wsId);
    }
  };

  // Initialize selectedWorkspace with query param if present, else first workspace
  useEffect(() => {
    if (displayWorkspaces.length > 0) {
      if (workspaceIdParam) {
        const found = displayWorkspaces.find(w => w.id === workspaceIdParam);
        if (found) {
          setSelectedWorkspace(found);
          return;
        }
      }
      if (bookingIdParam) {
        const found = displayWorkspaces.find(w => w.bookingId === bookingIdParam);
        if (found) {
          setSelectedWorkspace(found);
          return;
        }
      }
      if (!selectedWorkspace) {
        setSelectedWorkspace(displayWorkspaces[0]);
      }
    }
  }, [displayWorkspaces, selectedWorkspace, bookingIdParam, workspaceIdParam]);

  // Load details whenever selectedWorkspace changes
  const loadWorkspaceDetails = async (wsId: string) => {
    if (!wsId) return;

    if (wsId.startsWith('ws-mock')) {
      setConversation({ id: 'conv-mock-active', workspaceId: 'ws-mock-active' });
      const mockMsgs = getMockMessages(wsId).map(m => ({
        id: m.id,
        conversationId: 'conv-mock-active',
        senderUserId: m.senderId === 'pt-mock-1' ? (currentUser?.role === 'pt' ? currentUser?.id : 'pt-mock-1') : (currentUser?.role === 'pt' ? 'cust-mock-2' : currentUser?.id || 'cust-me'),
        senderName: m.senderName,
        content: m.text,
        createdAt: m.createdAt
      }));
      setMessages(mockMsgs);
      setUseMockChat(true);
      setMealPlansList([
        {
          id: 'meal-plan-mock-1',
          workspaceId: 'ws-mock-active',
          title: 'Thực Đơn AI: Tăng Cơ & Sức Mạnh',
          contentJson: JSON.stringify({
            dailyCalories: 2500,
            proteinGrams: 150,
            carbGrams: 280,
            fatGrams: 70,
            days: [
              {
                day: 1,
                meals: [
                  {
                    mealType: 'Bữa Sáng (07:30)',
                    foods: [
                      { name: 'Phở bò chín', quantity: '1 tô lớn', calories: 550, protein: 25, carbs: 65, fat: 18 },
                      { name: 'Trứng gà luộc', quantity: '2 quả', calories: 150, protein: 13, carbs: 1, fat: 10 }
                    ]
                  },
                  {
                    mealType: 'Bữa Phụ Sáng (10:00)',
                    foods: [
                      { name: 'Chuối gia', quantity: '1 quả lớn', calories: 100, protein: 1, carbs: 25, fat: 0 },
                      { name: 'Sữa tươi không đường', quantity: '200ml', calories: 120, protein: 7, carbs: 10, fat: 6 }
                    ]
                  },
                  {
                    mealType: 'Bữa Trưa (12:30)',
                    foods: [
                      { name: 'Cơm trắng', quantity: '2 chén', calories: 400, protein: 8, carbs: 88, fat: 1 },
                      { name: 'Ức gà áp chảo', quantity: '150g', calories: 250, protein: 45, carbs: 0, fat: 5 },
                      { name: 'Bông cải xanh luộc', quantity: '100g', calories: 35, protein: 3, carbs: 7, fat: 0 }
                    ]
                  },
                  {
                    mealType: 'Bữa Phụ Chiều - Trước Tập (16:00)',
                    foods: [
                      { name: 'Bánh mì đen', quantity: '2 lát', calories: 160, protein: 6, carbs: 32, fat: 2 },
                      { name: 'Lòng trắng trứng', quantity: '4 quả', calories: 68, protein: 16, carbs: 0, fat: 0 }
                    ]
                  },
                  {
                    mealType: 'Bữa Tối (19:30)',
                    foods: [
                      { name: 'Cơm trắng', quantity: '1.5 chén', calories: 300, protein: 6, carbs: 66, fat: 1 },
                      { name: 'Cá hồi áp chảo', quantity: '120g', calories: 220, protein: 25, carbs: 0, fat: 12 },
                      { name: 'Rau cải ngọt xào tỏi', quantity: '1 đĩa', calories: 80, protein: 2, carbs: 5, fat: 6 }
                    ]
                  }
                ]
              }
            ]
          }),
          isActive: true,
          source: 1,
          createdAt: new Date().toISOString()
        }
      ]);
      setLoadingMessages(false);
      setLoadingMealPlan(false);
      return;
    }

    let conv: any = null;
    try {
      setLoadingMessages(true);
      // 1. Get conversation by workspace ID
      const convRes = await api.get(`/conversations/workspace/${wsId}`);
      if (convRes.data?.success && convRes.data.data) {
        conv = convRes.data.data;
        setConversation(conv);
      } else {
        // Conversation doesn't exist yet, try to create it via bookingId
        const ws = displayWorkspaces.find(w => w.id === wsId);
        if (ws?.bookingId) {
          const createRes = await api.post(`/conversations/booking/${ws.bookingId}`);
          if (createRes.data?.success && createRes.data.data) {
            conv = createRes.data.data;
            setConversation(conv);
          }
        }
      }

      if (conv) {
        try {
          const msgRes = await api.get(`/conversations/${conv.id}/messages`);
          if (msgRes.data?.success && msgRes.data.data && msgRes.data.data.length > 0) {
            setMessages(msgRes.data.data);
            setUseMockChat(false);
          } else {
            setUseMockChat(true);
            const mockMsgs = getMockMessages(wsId).map(m => ({
              id: m.id,
              conversationId: conv.id,
              senderUserId: m.senderId === 'pt-1' || m.senderId === 'pt-mock-1' ? selectedWorkspace?.ptId : m.senderId,
              senderName: m.senderName,
              content: m.text,
              createdAt: m.createdAt
            }));
            setMessages(mockMsgs);
          }
        } catch (msgErr) {
          console.warn('Backend messages API failed, falling back to mock messages:', msgErr);
          setUseMockChat(true);
          const mockMsgs = getMockMessages(wsId).map(m => ({
            id: m.id,
            conversationId: conv.id,
            senderUserId: m.senderId === 'pt-1' || m.senderId === 'pt-mock-1' ? selectedWorkspace?.ptId : m.senderId,
            senderName: m.senderName,
            content: m.text,
            createdAt: m.createdAt
          }));
          setMessages(mockMsgs);
        }
      } else {
        setMessages([]);
      }
    } catch (e) {
      console.error('Error loading conversation/messages:', e);
      setMessages([]);
    } finally {
      setLoadingMessages(false);
    }

    try {
      setLoadingMealPlan(true);
      // 3. Load meal plans
      const mealRes = await api.get(`/meal-plans/workspace/${wsId}`);
      if (mealRes.data?.success && mealRes.data.data && mealRes.data.data.length > 0) {
        setMealPlansList(mealRes.data.data);
      } else {
        setMealPlansList([
          {
            id: 'meal-plan-mock-1',
            workspaceId: wsId,
            title: 'Thực Đơn AI: Tăng Cơ & Sức Mạnh',
            contentJson: JSON.stringify({
              dailyCalories: 2500,
              proteinGrams: 150,
              carbGrams: 280,
              fatGrams: 70,
              days: [
                {
                  day: 1,
                  meals: [
                    {
                      mealType: 'Bữa Sáng (07:30)',
                      foods: [
                        { name: 'Phở bò chín', quantity: '1 tô lớn', calories: 550, protein: 25, carbs: 65, fat: 18 },
                        { name: 'Trứng gà luộc', quantity: '2 quả', calories: 150, protein: 13, carbs: 1, fat: 10 }
                      ]
                    },
                    {
                      mealType: 'Bữa Phụ Sáng (10:00)',
                      foods: [
                        { name: 'Chuối gia', quantity: '1 quả lớn', calories: 100, protein: 1, carbs: 25, fat: 0 },
                        { name: 'Sữa tươi không đường', quantity: '200ml', calories: 120, protein: 7, carbs: 10, fat: 6 }
                      ]
                    },
                    {
                      mealType: 'Bữa Trưa (12:30)',
                      foods: [
                        { name: 'Cơm trắng', quantity: '2 chén', calories: 400, protein: 8, carbs: 88, fat: 1 },
                        { name: 'Ức gà áp chảo', quantity: '150g', calories: 250, protein: 45, carbs: 0, fat: 5 },
                        { name: 'Bông cải xanh luộc', quantity: '100g', calories: 35, protein: 3, carbs: 7, fat: 0 }
                      ]
                    },
                    {
                      mealType: 'Bữa Phụ Chiều - Trước Tập (16:00)',
                      foods: [
                        { name: 'Bánh mì đen', quantity: '2 lát', calories: 160, protein: 6, carbs: 32, fat: 2 },
                        { name: 'Lòng trắng trứng', quantity: '4 quả', calories: 68, protein: 16, carbs: 0, fat: 0 }
                      ]
                    },
                    {
                      mealType: 'Bữa Tối (19:30)',
                      foods: [
                        { name: 'Cơm trắng', quantity: '1.5 chén', calories: 300, protein: 6, carbs: 66, fat: 1 },
                        { name: 'Cá hồi áp chảo', quantity: '120g', calories: 220, protein: 25, carbs: 0, fat: 12 },
                        { name: 'Rau cải ngọt xào tỏi', quantity: '1 đĩa', calories: 80, protein: 2, carbs: 5, fat: 6 }
                      ]
                    }
                  ]
                }
              ]
            }),
            isActive: true,
            source: 1,
            createdAt: new Date().toISOString()
          }
        ]);
      }
    } catch (e) {
      console.error('Error loading meal plans:', e);
      setMealPlansList([
        {
          id: 'meal-plan-mock-1',
          workspaceId: wsId,
          title: 'Thực Đơn AI: Tăng Cơ & Sức Mạnh',
          contentJson: JSON.stringify({
            dailyCalories: 2500,
            proteinGrams: 150,
            carbGrams: 280,
            fatGrams: 70,
            days: [
              {
                day: 1,
                meals: [
                  {
                    mealType: 'Bữa Sáng (07:30)',
                    foods: [
                      { name: 'Phở bò chín', quantity: '1 tô lớn', calories: 550, protein: 25, carbs: 65, fat: 18 },
                      { name: 'Trứng gà luộc', quantity: '2 quả', calories: 150, protein: 13, carbs: 1, fat: 10 }
                    ]
                  },
                  {
                    mealType: 'Bữa Phụ Sáng (10:00)',
                    foods: [
                      { name: 'Chuối gia', quantity: '1 quả lớn', calories: 100, protein: 1, carbs: 25, fat: 0 },
                      { name: 'Sữa tươi không đường', quantity: '200ml', calories: 120, protein: 7, carbs: 10, fat: 6 }
                    ]
                  },
                  {
                    mealType: 'Bữa Trưa (12:30)',
                    foods: [
                      { name: 'Cơm trắng', quantity: '2 chén', calories: 400, protein: 8, carbs: 88, fat: 1 },
                      { name: 'Ức gà áp chảo', quantity: '150g', calories: 250, protein: 45, carbs: 0, fat: 5 },
                      { name: 'Bông cải xanh luộc', quantity: '100g', calories: 35, protein: 3, carbs: 7, fat: 0 }
                    ]
                  },
                  {
                    mealType: 'Bữa Phụ Chiều - Trước Tập (16:00)',
                    foods: [
                      { name: 'Bánh mì đen', quantity: '2 lát', calories: 160, protein: 6, carbs: 32, fat: 2 },
                      { name: 'Lòng trắng trứng', quantity: '4 quả', calories: 68, protein: 16, carbs: 0, fat: 0 }
                    ]
                  },
                  {
                    mealType: 'Bữa Tối (19:30)',
                    foods: [
                      { name: 'Cơm trắng', quantity: '1.5 chén', calories: 300, protein: 6, carbs: 66, fat: 1 },
                      { name: 'Cá hồi áp chảo', quantity: '120g', calories: 220, protein: 25, carbs: 0, fat: 12 },
                      { name: 'Rau cải ngọt xào tỏi', quantity: '1 đĩa', calories: 80, protein: 2, carbs: 5, fat: 6 }
                    ]
                  }
                ]
              }
            ]
          }),
          isActive: true,
          source: 1,
          createdAt: new Date().toISOString()
        }
      ]);
    } finally {
      setLoadingMealPlan(false);
    }
  };

  useEffect(() => {
    if (selectedWorkspace) {
      loadWorkspaceDetails(selectedWorkspace.id);
    }
  }, [selectedWorkspace]);

  // Scroll to bottom on new messages
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  // Message polling for real-time feel
  useEffect(() => {
    if (!conversation || useMockChat) return;
    const interval = setInterval(async () => {
      try {
        const msgRes = await api.get(`/conversations/${conversation.id}/messages`);
        if (msgRes.data?.success) {
          setMessages(msgRes.data.data || []);
        }
      } catch (e) {
        console.warn('Silent messages poll failed:', e);
      }
    }, 5000);
    return () => clearInterval(interval);
  }, [conversation, useMockChat]);

  const sendMessage = async () => {
    if (!input.trim() || !conversation || sendingMessage) return;
    const text = input.trim();
    setInput('');
    setSendingMessage(true);
    try {
      if (useMockChat) {
        throw new Error('Directly using mock chat');
      }
      const res = await api.post('/conversations/messages', {
        conversationId: conversation.id,
        content: text,
        messageType: 'text'
      });
      if (res.data?.success && res.data.data) {
        setMessages(prev => [...prev, res.data.data]);
      } else {
        throw new Error('SendMessage API response success is false');
      }
    } catch (e) {
      console.warn('Backend send message API failed/mock mode active, saving locally:', e);
      const mockMsg = {
        id: 'msg-' + Math.random().toString(36).substring(2, 9),
        conversationId: conversation.id,
        senderUserId: currentUser?.id,
        senderName: currentUser?.fullName || 'Me',
        content: text,
        createdAt: new Date().toISOString()
      };
      
      const allMsgs = getMockMessages();
      allMsgs.push({
        id: mockMsg.id,
        workspaceId: selectedWorkspace.id,
        senderId: mockMsg.senderUserId || 'user',
        senderName: mockMsg.senderName,
        text: mockMsg.content,
        createdAt: mockMsg.createdAt,
        read: true
      });
      saveMockMessages(allMsgs);
      setMessages(prev => [...prev, mockMsg]);

      // Trigger automatic PT response simulation
      if (currentUser?.role === 'customer') {
        setTimeout(() => {
          const responses = [
            `Tuyệt vời! Tôi đã cập nhật kế hoạch luyện tập của chúng ta. Bạn nhớ đăng nhập đầy đủ các buổi tập nhé.`,
            `Cố lên! Hãy bổ sung nước và protein đầy đủ cho ngày hôm nay nhé! 📈`,
            `Tôi đã nhận được thông tin. Để tôi xem lại chỉ số hình thể của bạn để thiết lập thực đơn AI cho phù hợp.`,
            `Nếu bạn thấy đau ở khớp hay cơ, hãy dừng tập ngay lập tức. Chúng ta sẽ điều chỉnh bài tập tập trung giãn cơ trước.`,
            `Hãy cố gắng ngủ đủ 7-8 tiếng tối nay nhé! Cần giúp đỡ gì thêm cứ nhắn tôi. 💪`
          ];
          const replyText = responses[Math.floor(Math.random() * responses.length)];
          const replyMsg = {
            id: 'msg-sim-' + Math.random().toString(36).substring(2, 9),
            conversationId: conversation.id,
            senderUserId: selectedWorkspace?.ptId || 'pt-1',
            senderName: selectedWorkspace?.ptName || 'HLV',
            content: replyText,
            createdAt: new Date().toISOString()
          };

          const updatedAllMsgs = getMockMessages();
          updatedAllMsgs.push({
            id: replyMsg.id,
            workspaceId: selectedWorkspace.id,
            senderId: replyMsg.senderUserId,
            senderName: replyMsg.senderName,
            text: replyMsg.content,
            createdAt: replyMsg.createdAt,
            read: false
          });
          saveMockMessages(updatedAllMsgs);
          setMessages(prev => [...prev, replyMsg]);
        }, 1800);
      }
    } finally {
      setSendingMessage(false);
    }
  };

  const handleGenerateMealPlan = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedWorkspace || generatingMealPlan) return;
    setGeneratingMealPlan(true);
    setGenError('');
    try {
      // Step 1: Request Gemini AI to generate JSON string
      const genRes = await api.post('/meal-plans/generate', {
        workspaceId: selectedWorkspace.id,
        goal,
        preference: preference || 'Dễ tìm, chuẩn vị Việt',
        allergyNote: allergyNote || 'Không'
      });

      if (genRes.data?.success && genRes.data.data) {
        const generatedJson = genRes.data.data;
        
        // Step 2: Save the generated plan to Database
        const createRes = await api.post('/meal-plans', {
          workspaceId: selectedWorkspace.id,
          title: `Thực Đơn AI: ${goal}`,
          contentJson: generatedJson,
          source: 1 // Source: 1 = AI generated
        });

        if (createRes.data?.success && createRes.data.data) {
          // Re-fetch meal plans
          const mealRes = await api.get(`/meal-plans/workspace/${selectedWorkspace.id}`);
          if (mealRes.data?.success) {
            setMealPlansList(mealRes.data.data || []);
          }
          // Reset states & close modal
          setGoal('');
          setPreference('');
          setAllergyNote('');
          setShowCreatePlanModal(false);
          setActiveTab('mealplan');
        } else {
          setGenError(createRes.data?.message || 'Không thể lưu thực đơn vào cơ sở dữ liệu.');
        }
      } else {
        setGenError(genRes.data?.message || 'Gemini AI không thể tạo thực đơn vào lúc này.');
      }
    } catch (err: any) {
      console.error('Failed to generate meal plan:', err);
      setGenError(err.response?.data?.message || err.message || 'Lỗi kết nối máy chủ.');
    } finally {
      setGeneratingMealPlan(false);
    }
  };

  const handleActivatePlan = async (planId: string) => {
    try {
      const res = await api.patch(`/meal-plans/${planId}/activate`);
      if (res.data?.success) {
        // Refresh plans list
        if (selectedWorkspace) {
          const mealRes = await api.get(`/meal-plans/workspace/${selectedWorkspace.id}`);
          if (mealRes.data?.success) {
            setMealPlansList(mealRes.data.data || []);
          }
        }
      }
    } catch (e) {
      console.error('Error activating plan:', e);
    }
  };

  const handleDeletePlan = async (planId: string) => {
    if (!window.confirm('Bạn có chắc muốn xóa thực đơn này không?')) return;
    try {
      const res = await api.delete(`/meal-plans/${planId}`);
      if (res.data?.success) {
        // Refresh plans list
        if (selectedWorkspace) {
          const mealRes = await api.get(`/meal-plans/workspace/${selectedWorkspace.id}`);
          if (mealRes.data?.success) {
            setMealPlansList(mealRes.data.data || []);
          }
        }
      }
    } catch (e) {
      console.error('Error deleting plan:', e);
    }
  };

  // Find the active meal plan (fallback to the first one)
  const activePlan = mealPlansList.find(p => p.isActive) || mealPlansList[0];
  
  let parsedPlan = null;
  if (activePlan && activePlan.contentJson) {
    try {
      parsedPlan = JSON.parse(activePlan.contentJson);
    } catch (e) {
      console.error('Error parsing plan json:', e);
    }
  }

  const calories = parsedPlan?.dailyCalories || parsedPlan?.calories || 0;
  const protein = parsedPlan?.proteinGrams || parsedPlan?.protein || 0;
  const carbs = parsedPlan?.carbGrams || parsedPlan?.carbs || 0;
  const fat = parsedPlan?.fatGrams || parsedPlan?.fat || 0;
  const daysList = parsedPlan?.days || [];
  const currentDay = daysList[selectedDayIndex] || daysList[0];
  const mealsList = currentDay?.meals || [];

  const macroPercent = (val: number, total: number) => {
    if (!total) return 0;
    return Math.round((val / total) * 100);
  };

  return (
    <div className="w-full bg-brand-black py-4 h-[calc(100vh-70px)] flex flex-col mt-[70px]">
      <div className="section-container flex-1 flex flex-col h-full overflow-hidden">
        <div className="flex-1 flex bg-brand-dark border border-brand-border overflow-hidden h-full">
      {/* Workspace Sidebar / Conversations List */}
      <aside className="w-72 flex-shrink-0 bg-brand-dark border-r border-brand-border flex flex-col hidden md:flex">
        <div className="px-5 py-5 border-b border-brand-border">
          <p className="section-label">
            {currentUser?.role === 'pt' ? 'Danh Sách Học Viên' : 'Danh Sách Trò Chuyện'}
          </p>
          <h2 className="font-montserrat font-bold text-xl text-white uppercase tracking-wider mt-1">TRÒ CHUYỆN CHI TIẾT</h2>
        </div>

        <div className="flex-1 overflow-y-auto">
          {displayWorkspaces.length === 0 ? (
            <div className="p-5 text-center text-white/30 text-xs">
              {currentUser?.role === 'pt' ? 'Chưa có học viên nào hoạt động' : 'Chưa có khóa học nào hoạt động'}
            </div>
          ) : (
            displayWorkspaces.map((ws) => {
              const isActive = selectedWorkspace && ws.id === selectedWorkspace.id;
              const name = currentUser?.role === 'pt' ? ws.customerName : ws.ptName;
              const initials = name ? name.split(' ').map((n: string) => n[0]).join('').substring(0, 2).toUpperCase() : 'WS';
              const specialty = ws.packageName || 'Khóa tập liên kết';
              
              return (
                <button
                  key={ws.id}
                  onClick={() => setSelectedWorkspace(ws)}
                  className={`w-full text-left px-5 py-4 border-b border-brand-border cursor-pointer transition-all duration-200 border-l-2 ${
                    isActive ? 'bg-brand-red/10 border-l-brand-red' : 'border-l-transparent hover:bg-white/[0.03] hover:border-l-brand-border'
                  }`}
                >
                  <div className="flex items-center gap-3">
                    <div className={`w-10 h-10 flex-shrink-0 flex items-center justify-center border ${isActive ? 'bg-brand-red border-brand-red' : 'bg-brand-surface border-brand-border'}`}>
                      <span className="font-display text-sm text-white font-bold">{initials}</span>
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className={`text-sm font-semibold truncate ${isActive ? 'text-white' : 'text-white/70'}`}>{name}</p>
                      <p className="text-white/30 text-xs truncate mt-0.5">{specialty}</p>
                    </div>
                  </div>
                </button>
              );
            })
          )}
        </div>
      </aside>

      {/* Main Chat Area */}
      {selectedWorkspace ? (
        <main className="flex-1 flex flex-col overflow-hidden">
          {/* Chat Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-brand-border bg-brand-dark flex-shrink-0">
            <div className="flex items-center gap-3">
              <div className="w-10 h-10 bg-brand-red flex items-center justify-center font-bold">
                <span className="font-display text-base text-white">
                  {(currentUser?.role === 'pt' ? selectedWorkspace.customerName : selectedWorkspace.ptName)
                    ?.split(' ').map((n: string) => n[0]).join('').substring(0, 2).toUpperCase() || 'WS'}
                </span>
              </div>
              <div>
                <p className="text-white font-semibold text-sm">
                  {currentUser?.role === 'pt' ? selectedWorkspace.customerName : selectedWorkspace.ptName}
                </p>
                <p className="text-white/30 text-xs">{selectedWorkspace.packageName || 'Khóa Tập Luyện'}</p>
              </div>
            </div>
            
            <div className="flex items-center gap-3">
              {/* Progress Text */}
              <div className="text-right mr-2 hidden sm:block">
                <p className="text-white/40 text-[9px] uppercase tracking-wider">Tiến Độ Lớp Học</p>
                <p className="text-white font-bold text-xs font-mono">
                  {selectedWorkspace.sessionsCompleted} / {selectedWorkspace.sessionsTotal} Buổi
                </p>
              </div>

              {/* HLV / PT Action: Increment Session */}
              {currentUser?.role === 'pt' && selectedWorkspace.status === 'active' && (
                <button
                  onClick={() => {
                    const nextSess = selectedWorkspace.sessionsCompleted + 1;
                    if (nextSess > selectedWorkspace.sessionsTotal) {
                      alert("Khóa học đã hoàn thành tất cả các buổi!");
                      return;
                    }
                    if (window.confirm(`Gửi yêu cầu hoàn thành (Checkout) buổi tập thứ ${nextSess}?`)) {
                      sendCheckoutRequest(selectedWorkspace.id, nextSess);
                    }
                  }}
                  className="flex items-center gap-1.5 px-3 py-2 text-xs font-semibold uppercase tracking-wider cursor-pointer border border-brand-red bg-brand-red/10 text-brand-red hover:bg-brand-red hover:text-white transition-all duration-200"
                >
                  <Plus size={12} />
                  + Yêu Cầu Checkout (Buổi {selectedWorkspace.sessionsCompleted + 1})
                </button>
              )}



              {/* Customer Action: Dispute */}
              {currentUser?.role === 'customer' && selectedWorkspace.status === 'active' && (
                <button
                  onClick={() => {
                    setDisputeReason('');
                    setDisputeDesc('');
                    setDisputeFile(null);
                    setShowDisputeModal(true);
                  }}
                  className="flex items-center gap-1.5 px-3 py-2 text-xs font-semibold uppercase tracking-wider cursor-pointer border border-white/20 hover:border-brand-red text-white/60 hover:text-white hover:bg-brand-red/10 transition-all duration-200"
                >
                  Khiếu Nại
                </button>
              )}
            </div>
          </div>

          {/* TAB NAVIGATION */}
          <div className="flex border-b border-brand-border bg-brand-surface/30 px-6">
            <button
              onClick={() => setActiveTab('sessions')}
              className={`px-6 py-3 text-xs font-bold uppercase tracking-widest transition-all duration-200 border-b-2 flex items-center gap-2 ${
                activeTab === 'sessions'
                  ? 'border-brand-red text-white bg-white/[0.02]'
                  : 'border-transparent text-white/40 hover:text-white hover:bg-white/[0.02]'
              }`}
            >
              <Target size={14} className={activeTab === 'sessions' ? 'text-brand-red' : 'text-white/40'} />
              Tiến Độ ({selectedWorkspace.sessionsCompleted}/{selectedWorkspace.sessionsTotal})
            </button>
            <button
              onClick={() => setActiveTab('chat')}
              className={`px-6 py-3 text-xs font-bold uppercase tracking-widest transition-all duration-200 border-b-2 flex items-center gap-2 ${
                activeTab === 'chat'
                  ? 'border-brand-red text-white bg-white/[0.02]'
                  : 'border-transparent text-white/40 hover:text-white hover:bg-white/[0.02]'
              }`}
            >
              <MessageSquare size={14} className={activeTab === 'chat' ? 'text-brand-red' : 'text-white/40'} />
              Nhắn Tin
            </button>
            <button
              onClick={() => setActiveTab('mealplan')}
              className={`px-6 py-3 text-xs font-bold uppercase tracking-widest transition-all duration-200 border-b-2 flex items-center gap-2 ${
                activeTab === 'mealplan'
                  ? 'border-brand-red text-white bg-white/[0.02]'
                  : 'border-transparent text-white/40 hover:text-white hover:bg-white/[0.02]'
              }`}
            >
              <Zap size={14} className={activeTab === 'mealplan' ? 'text-brand-red' : 'text-white/40'} />
              Thực Đơn AI
            </button>
          </div>

          <div className="flex-1 flex overflow-hidden relative bg-brand-black">
            {/* Chat Tab */}
            {activeTab === 'chat' && (
              <div className="absolute inset-0 flex flex-col w-full h-full animate-fade-in">
                <div className="flex-1 overflow-y-auto px-6 py-6 flex flex-col gap-4">
              {loadingMessages ? (
                <div className="flex-1 flex items-center justify-center flex-col gap-2 text-white/40">
                  <Loader2 className="animate-spin text-brand-red" size={24} />
                  <span className="text-xs">Đang tải tin nhắn...</span>
                </div>
              ) : messages.length === 0 ? (
                <div className="flex-1 flex items-center justify-center text-white/20 text-xs">
                  Gửi tin nhắn đầu tiên để bắt đầu cuộc trò chuyện
                </div>
              ) : (
                messages.map((msg) => {
                  const isMe = msg.senderUserId === currentUser?.id;
                  const checkoutMatch = msg.content.match(/^\[CheckoutSession:(\d+):(.*)\]$/);
                  
                  if (checkoutMatch) {
                    const sessionNum = parseInt(checkoutMatch[1]);
                    const dateStr = checkoutMatch[2];
                    const confirmedKey = `${selectedWorkspace.id}-${sessionNum}`;
                    const isConfirmed = confirmedSessions.includes(confirmedKey);
                    const isNextSession = sessionNum === selectedWorkspace.sessionsCompleted + 1;
                    const isPastSession = sessionNum <= selectedWorkspace.sessionsCompleted;
                    
                    return (
                      <div key={msg.id} className="flex justify-center my-4 w-full">
                        <div className="bg-brand-dark border border-brand-red/30 p-5 rounded-lg max-w-md w-full shadow-lg shadow-brand-red/5 flex flex-col items-center text-center gap-3 relative overflow-hidden">
                          {/* Glow decoration */}
                          <div className="absolute -top-12 -left-12 w-24 h-24 bg-brand-red/10 rounded-full blur-xl pointer-events-none" />
                          <div className="absolute -bottom-12 -right-12 w-24 h-24 bg-brand-red/10 rounded-full blur-xl pointer-events-none" />
                          
                          <div className="flex items-center justify-center w-12 h-12 rounded-full bg-brand-red/10 border border-brand-red/30 text-brand-red">
                            <Dumbbell size={20} className={!isConfirmed && isNextSession ? "animate-pulse" : ""} />
                          </div>
                          
                          <div>
                            <h4 className="font-montserrat font-bold text-xs text-white uppercase tracking-wider">
                              🔔 YÊU CẦU CHECKOUT BUỔI TẬP
                            </h4>
                            <p className="text-white/60 text-[11px] mt-1">
                              Xác nhận hoàn thành <strong>Buổi tập thứ {sessionNum}</strong>
                            </p>
                            <span className="text-[9px] text-white/30 block mt-0.5 font-mono">Tạo ngày: {dateStr}</span>
                          </div>
                          
                          {/* Clickable Number */}
                          <div className="my-2">
                            {isConfirmed || isPastSession ? (
                              <button
                                disabled
                                className="w-14 h-14 rounded-full bg-green-500/10 border border-green-500/30 text-green-500 flex flex-col items-center justify-center cursor-not-allowed"
                              >
                                <span className="text-base font-bold font-mono leading-none">{sessionNum}</span>
                                <span className="text-[8px] uppercase tracking-wider font-bold mt-1">XONG</span>
                              </button>
                            ) : (
                              <button
                                onClick={() => handleConfirmSession(selectedWorkspace.id, sessionNum)}
                                disabled={!isNextSession}
                                className={`w-14 h-14 rounded-full flex flex-col items-center justify-center transition-all duration-300 ${
                                  isNextSession
                                    ? 'bg-brand-red/20 border border-brand-red text-white hover:bg-brand-red hover:text-white hover:scale-105 cursor-pointer shadow-md shadow-brand-red/20'
                                    : 'bg-brand-surface border border-brand-border text-white/20 cursor-not-allowed'
                                }`}
                                title={isNextSession ? "Nhấn vào số để xác nhận hoàn thành" : `Cần hoàn thành buổi tập thứ ${selectedWorkspace.sessionsCompleted + 1} trước`}
                              >
                                <span className="text-base font-bold font-mono leading-none">{sessionNum}</span>
                                <span className="text-[8px] uppercase tracking-wider font-bold mt-1">XÁC NHẬN</span>
                              </button>
                            )}
                          </div>
                          
                          <div className="text-[10px]">
                            {isConfirmed || isPastSession ? (
                              <span className="text-green-500 font-semibold">
                                ✓ Đã hoàn thành
                              </span>
                            ) : isNextSession ? (
                              <span className="text-brand-red font-semibold animate-pulse">
                                ● Đang chờ xác nhận (Nhấp vào số)
                              </span>
                            ) : (
                              <span className="text-white/30 italic">
                                Chưa thể xác nhận (Chưa tới lượt)
                              </span>
                            )}
                          </div>
                        </div>
                      </div>
                    );
                  }

                  return (
                    <div key={msg.id} className={`flex ${isMe ? 'justify-end' : 'justify-start'}`}>
                      <div className={`max-w-[70%] flex flex-col gap-1 ${isMe ? 'items-end' : 'items-start'}`}>
                        {!isMe && (
                          <span className="text-white/30 text-[10px] px-1 font-semibold">{msg.senderName}</span>
                        )}
                        <div className={`px-5 py-3 rounded-[20px] text-sm leading-relaxed shadow-lg ${
                          isMe
                            ? 'bg-brand-red text-white rounded-br-sm'
                            : 'bg-brand-surface border border-brand-border text-white/90 rounded-bl-sm'
                        }`}>
                          {msg.content}
                        </div>
                        <span className="text-white/20 text-[9px] px-1">
                          {new Date(msg.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                        </span>
                      </div>
                    </div>
                  );
                })
              )}
              <div ref={messagesEndRef} />
            </div>

            {/* Message Input Bar */}
            <div className="flex-shrink-0 border-t border-brand-border bg-brand-dark px-6 py-4">
              <div className="flex items-center gap-3">
                <div className="flex-1 relative">
                  <input
                    type="text"
                    value={input}
                    onChange={(e) => setInput(e.target.value)}
                    onKeyDown={(e) => e.key === 'Enter' && !e.shiftKey && sendMessage()}
                    placeholder="Nhập tin nhắn để thảo luận với HLV/Học viên..."
                    className="input-dark pr-4"
                    disabled={sendingMessage}
                  />
                </div>
                <button
                  onClick={sendMessage}
                  disabled={!input.trim() || sendingMessage}
                  className={`w-12 h-12 flex items-center justify-center transition-all duration-200 ${
                    input.trim() && !sendingMessage
                      ? 'bg-brand-red hover:bg-brand-red-dark cursor-pointer'
                      : 'bg-brand-border cursor-not-allowed'
                  }`}
                >
                  {sendingMessage ? (
                    <Loader2 className="animate-spin text-white" size={16} />
                  ) : (
                    <Send size={16} className="text-white" />
                  )}
                </button>
              </div>
              <p className="text-white/20 text-[10px] mt-2 uppercase tracking-widest">Nhấn Enter để gửi tin nhắn</p>
            </div>
            </div>
            )}

            {/* Meal Plan Tab */}
            {activeTab === 'mealplan' && (
              <div className="absolute inset-0 w-full h-full overflow-y-auto bg-brand-black animate-fade-in flex flex-col lg:flex-row">
                <div className="flex-1 max-w-4xl mx-auto w-full p-6">
                  <div className="flex items-center justify-between mb-8">
                    <div>
                      <div className="flex items-center gap-2 mb-2">
                        <Zap size={20} className="text-brand-red" />
                        <h2 className="font-montserrat font-bold text-2xl text-white uppercase tracking-wider">Hệ Thống Dinh Dưỡng AI</h2>
                      </div>
                      <p className="text-white/40 text-sm">Thiết kế thực đơn cá nhân hóa dựa trên mục tiêu và thể trạng.</p>
                    </div>

                    {currentUser?.role === 'pt' && (
                      <button
                        onClick={() => setShowCreatePlanModal(true)}
                        className="btn-primary flex items-center gap-2 py-3 px-6"
                      >
                        <Plus size={18} />
                        TẠO THỰC ĐƠN MỚI
                      </button>
                    )}
                  </div>

                  {loadingMealPlan ? (
                    <div className="p-12 text-center text-white/40 flex flex-col items-center gap-4">
                      <Loader2 className="animate-spin text-brand-red" size={32} />
                      <span>Đang phân tích dữ liệu dinh dưỡng...</span>
                    </div>
                  ) : mealPlansList.length === 0 ? (
                    <div className="border border-brand-border bg-brand-surface/30 p-12 text-center rounded-xl backdrop-blur-md">
                      <Zap size={48} className="text-white/10 mx-auto mb-4" />
                      {currentUser?.role === 'pt' ? (
                        <div className="flex flex-col gap-4 items-center">
                          <p className="text-white/60">Học viên này chưa có thực đơn nào. Hãy sử dụng Gemini AI để thiết kế ngay!</p>
                        </div>
                      ) : (
                        <p className="text-white/60">Huấn luyện viên chưa thiết lập thực đơn dinh dưỡng cho bạn.</p>
                      )}
                    </div>
                  ) : (
                    <div className="flex flex-col lg:flex-row gap-8">
                      {/* Left Column: Menu Switcher */}
                      <div className="w-full lg:w-72 flex-shrink-0 space-y-4">
                        <div className="glass-panel p-4">
                          <label className="block text-xs text-brand-red uppercase tracking-widest mb-3 font-bold border-b border-brand-red/20 pb-2">Các Bản Thực Đơn</label>
                          <div className="flex flex-col gap-2">
                            {mealPlansList.map((plan) => (
                              <div key={plan.id} className={`flex flex-col p-3 border transition-all ${plan.isActive ? 'bg-brand-red/10 border-brand-red shadow-[0_0_15px_rgba(255,51,51,0.1)]' : 'bg-brand-surface border-brand-border hover:border-white/20'}`}>
                                <div className="flex items-start justify-between gap-2 mb-2">
                                  <span className={`text-sm font-semibold leading-tight ${plan.isActive ? 'text-white' : 'text-white/60'}`}>{plan.title}</span>
                                  {plan.isActive && (
                                    <span className="text-[9px] bg-brand-red text-white px-1.5 py-0.5 rounded-md font-bold tracking-wider flex-shrink-0">ACTIVE</span>
                                  )}
                                </div>
                                
                                <div className="flex items-center justify-between mt-1">
                                  <span className="text-[10px] text-white/30 font-mono">
                                    {new Date(plan.createdAt).toLocaleDateString('vi-VN')}
                                  </span>
                                  <div className="flex items-center gap-2">
                                    {!plan.isActive && (
                                      <button
                                        onClick={() => handleActivatePlan(plan.id)}
                                        className="text-[10px] uppercase font-bold text-white/40 hover:text-white transition-colors"
                                      >
                                        Áp Dụng
                                      </button>
                                    )}
                                    {currentUser?.role === 'pt' && (
                                      <button
                                        onClick={() => handleDeletePlan(plan.id)}
                                        className="text-white/20 hover:text-brand-red transition-colors"
                                      >
                                        <Trash2 size={14} />
                                      </button>
                                    )}
                                  </div>
                                </div>
                              </div>
                            ))}
                          </div>
                        </div>

                        {/* Macros Summary */}
                        {parsedPlan && (
                          <div className="glass-panel p-5">
                            <h4 className="text-xs text-white/50 uppercase tracking-widest font-bold mb-4">Tổng Quan Dinh Dưỡng</h4>
                            <div className="grid grid-cols-2 gap-3 mb-5">
                              {[
                                { label: 'Calo Mục Tiêu', value: calories, unit: 'kcal', color: 'text-white' },
                                { label: 'Protein', value: protein, unit: 'g', color: 'text-blue-400' },
                                { label: 'Carbs', value: carbs, unit: 'g', color: 'text-yellow-400' },
                                { label: 'Fat', value: fat, unit: 'g', color: 'text-brand-red' },
                              ].map((m, i) => (
                                <div key={i} className="bg-brand-black/50 border border-brand-border/50 p-3 rounded-lg text-center">
                                  <p className={`font-bold text-xl font-mono ${m.color}`}>{m.value}</p>
                                  <p className="text-white/40 text-[9px] uppercase tracking-wider mt-1">{m.label}</p>
                                </div>
                              ))}
                            </div>

                            <div className="space-y-3">
                              {[
                                { label: 'Chất Đạm (Protein)', val: macroPercent(protein * 4, calories), color: 'bg-blue-400' },
                                { label: 'Tinh Bột (Carbs)', val: macroPercent(carbs * 4, calories), color: 'bg-yellow-400' },
                                { label: 'Chất Béo (Fat)', val: macroPercent(fat * 9, calories), color: 'bg-brand-red' },
                              ].map((m, i) => (
                                <div key={i}>
                                  <div className="flex justify-between mb-1">
                                    <span className="text-white/50 text-[10px] uppercase tracking-wider">{m.label}</span>
                                    <span className="text-white/80 text-[10px] font-mono">{m.val}%</span>
                                  </div>
                                  <div className="h-1.5 bg-brand-black rounded-full overflow-hidden">
                                    <div className={`h-full ${m.color}`} style={{ width: `${m.val}%` }} />
                                  </div>
                                </div>
                              ))}
                            </div>
                          </div>
                        )}
                      </div>

                      {/* Right Column: Meal Details */}
                      {parsedPlan && (
                        <div className="flex-1 glass-panel p-6">
                          {daysList.length > 1 && (
                            <div className="flex gap-2 overflow-x-auto pb-4 mb-4 border-b border-white/5 scrollbar-thin">
                              {daysList.map((dayObj: any, index: number) => (
                                <button
                                  key={index}
                                  onClick={() => setSelectedDayIndex(index)}
                                  className={`px-4 py-2 text-sm font-bold transition-all rounded-md flex-shrink-0 ${
                                    selectedDayIndex === index
                                      ? 'bg-brand-red text-white shadow-lg shadow-brand-red/20'
                                      : 'bg-brand-surface text-white/40 hover:text-white hover:bg-white/5'
                                  }`}
                                >
                                  Ngày {dayObj.day || index + 1}
                                </button>
                              ))}
                            </div>
                          )}

                          <div className="space-y-6">
                            {mealsList.length === 0 ? (
                              <p className="text-center text-white/30 text-sm py-10">Không có dữ liệu bữa ăn cho ngày này.</p>
                            ) : (
                              mealsList.map((meal: any, i: number) => (
                                <div key={i} className="group">
                                  <h4 className="text-brand-red font-montserrat font-bold text-sm uppercase tracking-wider mb-3 flex items-center gap-2">
                                    <span className="w-1.5 h-1.5 bg-brand-red rounded-full"></span>
                                    {meal.mealType || `Bữa ${i + 1}`}
                                  </h4>
                                  <div className="grid gap-3">
                                    {meal.foods?.map((food: any, j: number) => (
                                      <div key={j} className="bg-brand-surface/40 hover:bg-brand-surface border border-brand-border/40 hover:border-brand-border p-4 transition-colors rounded-lg flex flex-col sm:flex-row sm:items-center justify-between gap-4">
                                        <div>
                                          <p className="text-white font-semibold text-base">{food.name}</p>
                                          <p className="text-brand-red/80 font-mono text-sm mt-0.5">{food.quantity}</p>
                                        </div>
                                        <div className="flex gap-4 text-[11px] font-mono bg-brand-black/50 px-3 py-2 rounded-md border border-white/5">
                                          <div className="flex flex-col items-center">
                                            <span className="text-white/30 uppercase text-[9px] mb-0.5">Calo</span>
                                            <span className="text-white">{food.calories}</span>
                                          </div>
                                          <div className="w-px bg-white/10"></div>
                                          <div className="flex flex-col items-center">
                                            <span className="text-white/30 uppercase text-[9px] mb-0.5">Pro</span>
                                            <span className="text-blue-400">{food.protein}g</span>
                                          </div>
                                          <div className="w-px bg-white/10"></div>
                                          <div className="flex flex-col items-center">
                                            <span className="text-white/30 uppercase text-[9px] mb-0.5">Carb</span>
                                            <span className="text-yellow-400">{food.carbs}g</span>
                                          </div>
                                          <div className="w-px bg-white/10"></div>
                                          <div className="flex flex-col items-center">
                                            <span className="text-white/30 uppercase text-[9px] mb-0.5">Fat</span>
                                            <span className="text-brand-red">{food.fat}g</span>
                                          </div>
                                        </div>
                                      </div>
                                    ))}
                                  </div>
                                </div>
                              ))
                            )}
                          </div>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>
            )}

            {/* Sessions Tab */}
            {activeTab === 'sessions' && (
              <div className="absolute inset-0 w-full h-full overflow-y-auto bg-brand-black animate-fade-in p-6">
                <div className="max-w-3xl mx-auto w-full">
                  <div className="mb-8">
                    <h2 className="font-montserrat font-bold text-2xl text-white uppercase tracking-wider mb-2">Tiến Độ Khóa Học</h2>
                    <p className="text-white/40 text-sm">Quản lý và theo dõi các buổi tập đã hoàn thành trong khóa {selectedWorkspace.packageName}.</p>
                  </div>

                  <div className="glass-panel p-8">
                    <div className="flex items-center justify-between mb-6">
                      <div>
                        <p className="text-4xl font-montserrat font-bold text-white">
                          {selectedWorkspace.sessionsCompleted} <span className="text-xl text-white/30">/ {selectedWorkspace.sessionsTotal}</span>
                        </p>
                        <p className="text-brand-red text-xs uppercase tracking-widest font-bold mt-1">Buổi Đã Tập</p>
                      </div>
                      <div className="w-24 h-24 rounded-full border-4 border-brand-surface flex items-center justify-center relative">
                        <svg className="absolute inset-0 w-full h-full -rotate-90">
                          <circle cx="44" cy="44" r="42" fill="none" stroke="currentColor" strokeWidth="4" className="text-brand-surface" />
                          <circle cx="44" cy="44" r="42" fill="none" stroke="currentColor" strokeWidth="4" className="text-brand-red" strokeDasharray="264" strokeDashoffset={264 - (264 * selectedWorkspace.sessionsCompleted / selectedWorkspace.sessionsTotal)} strokeLinecap="round" />
                        </svg>
                        <span className="font-bold text-xl">{Math.round((selectedWorkspace.sessionsCompleted / selectedWorkspace.sessionsTotal) * 100)}%</span>
                      </div>
                    </div>

                    <div className="grid grid-cols-4 sm:grid-cols-6 md:grid-cols-8 gap-3 mt-8">
                      {Array.from({ length: selectedWorkspace.sessionsTotal }).map((_, i) => {
                        const sessionNum = i + 1;
                        const isCompleted = sessionNum <= selectedWorkspace.sessionsCompleted;
                        const isNext = sessionNum === selectedWorkspace.sessionsCompleted + 1;
                        
                        return (
                          <div
                            key={i}
                            className={`aspect-square flex flex-col items-center justify-center rounded-lg border-2 transition-all duration-300 ${
                              isCompleted
                                ? 'bg-brand-red/20 border-brand-red text-white shadow-[0_0_15px_rgba(255,51,51,0.2)]'
                                : isNext
                                ? 'bg-brand-surface border-white/20 text-white animate-pulse'
                                : 'bg-brand-black border-brand-border text-white/20'
                            }`}
                          >
                            <span className="font-mono text-lg font-bold">{sessionNum}</span>
                            {isCompleted && <span className="text-[8px] uppercase tracking-wider text-brand-red font-bold mt-1">Xong</span>}
                            {isNext && <span className="text-[8px] uppercase tracking-wider text-white/50 mt-1">Tiếp Theo</span>}
                          </div>
                        );
                      })}
                    </div>
                  </div>
                </div>
              </div>
            )}
          </div>
        </main>
      ) : (
        <div className="flex-1 flex flex-col items-center justify-center text-white/40 bg-brand-black p-8">
          <Dumbbell size={48} className="text-brand-border mb-4 animate-pulse" />
          <h2 className="font-montserrat font-bold text-lg text-white uppercase tracking-wider">Không Tìm Thấy Không Gian Trò Chuyện</h2>
          <p className="text-xs text-white/30 text-center max-w-sm mt-2">
            {currentUser?.role === 'pt'
              ? 'Bạn chưa có học viên nào đang hoạt động. Khi học viên đăng ký gói tập của bạn và thanh toán thành công, không gian trao đổi sẽ tự động xuất hiện tại đây!'
              : 'Bạn chưa đăng ký khóa tập nào đang hoạt động hoặc cuộc giao dịch chưa được HLV phê duyệt. Hãy đăng ký khóa học ở phần Tìm PT!'}
          </p>
        </div>
      )}
        </div>
      </div>

      {/* PT AI Meal Plan Modal */}
      {showCreatePlanModal && (
        <div className="fixed inset-0 bg-black/80 flex items-center justify-center p-4 z-50 animate-fade-in">
          <div className="bg-brand-dark border-2 border-brand-border p-6 w-full max-w-md">
            <div className="flex items-center gap-2 mb-4">
              <Zap className="text-brand-red" size={20} />
              <h3 className="font-montserrat font-bold text-lg text-white uppercase tracking-wider">Tạo Thực Đơn Dinh Dưỡng AI</h3>
            </div>

            {genError && (
              <div className="mb-4 border border-brand-red bg-brand-red/10 px-3 py-2 text-brand-red text-xs">
                {genError}
              </div>
            )}

            <form onSubmit={handleGenerateMealPlan} className="flex flex-col gap-4">
              <div>
                <label className="block text-[10px] text-white/40 uppercase tracking-widest mb-1.5 font-bold">Mục Tiêu Tập Luyện</label>
                <input
                  type="text"
                  value={goal}
                  onChange={(e) => setGoal(e.target.value)}
                  placeholder="Ví dụ: Tăng cân tăng cơ, giảm mỡ đùi..."
                  required
                  className="input-dark"
                />
              </div>

              <div>
                <label className="block text-[10px] text-white/40 uppercase tracking-widest mb-1.5 font-bold">Sở Thích Ăn Uống</label>
                <input
                  type="text"
                  value={preference}
                  onChange={(e) => setPreference(e.target.value)}
                  placeholder="Ví dụ: Đồ ăn Việt, nhiều gà và rau xanh..."
                  className="input-dark"
                />
              </div>

              <div>
                <label className="block text-[10px] text-white/40 uppercase tracking-widest mb-1.5 font-bold">Dị Ứng & Kiêng Kỵ</label>
                <input
                  type="text"
                  value={allergyNote}
                  onChange={(e) => setAllergyNote(e.target.value)}
                  placeholder="Ví dụ: Không ăn tôm, dị ứng lạc..."
                  className="input-dark"
                />
              </div>

              <div className="flex gap-3 justify-end mt-4">
                <button
                  type="button"
                  onClick={() => setShowCreatePlanModal(false)}
                  className="px-4 py-2 border border-brand-border text-white/60 hover:text-white text-xs font-semibold uppercase tracking-wider cursor-pointer transition-colors"
                >
                  HỦY
                </button>
                <button
                  type="submit"
                  disabled={generatingMealPlan}
                  className="btn-primary py-2 px-4 text-xs tracking-wider justify-center min-w-[120px]"
                >
                  {generatingMealPlan ? (
                    <div className="flex items-center gap-1.5">
                      <Loader2 className="animate-spin text-white" size={12} />
                      ĐANG TẠO...
                    </div>
                  ) : (
                    'XÁC NHẬN TẠO'
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}

      {/* Customer Dispute Modal */}
      {showDisputeModal && (
        <div className="fixed inset-0 bg-black/80 flex items-center justify-center p-4 z-50 animate-fade-in">
          <div className="bg-brand-dark border-2 border-brand-border p-6 w-full max-w-md">
            <div className="flex items-center gap-2 mb-4">
              <span className="w-2.5 h-2.5 bg-brand-red rounded-full animate-ping" />
              <h3 className="font-montserrat font-bold text-lg text-white uppercase tracking-wider">Khiếu Nại Khóa Học</h3>
            </div>

            <form
              onSubmit={async (e) => {
                e.preventDefault();
                if (!disputeReason.trim() || !disputeDesc.trim()) {
                  alert('Vui lòng điền đầy đủ lý do và mô tả khiếu nại!');
                  return;
                }
                setSubmittingDispute(true);
                try {
                  await fileDispute(
                    selectedWorkspace.id,
                    disputeReason.trim(),
                    disputeDesc.trim(),
                    disputeFile || undefined
                  );
                  alert('Gửi khiếu nại thành công! Admin sẽ tiến hành kiểm duyệt.');
                  setShowDisputeModal(false);
                } catch (err) {
                  console.error(err);
                  alert('Gửi khiếu nại thất bại.');
                } finally {
                  setSubmittingDispute(false);
                }
              }}
              className="flex flex-col gap-4"
            >
              <div>
                <label className="block text-[10px] text-white/40 uppercase tracking-widest mb-1.5 font-bold">Lý Do Khiếu Nại</label>
                <input
                  type="text"
                  value={disputeReason}
                  onChange={(e) => setDisputeReason(e.target.value)}
                  placeholder="Ví dụ: HLV không dạy đúng lịch, thái độ thiếu chuyên nghiệp..."
                  required
                  className="input-dark focus:border-brand-red"
                  disabled={submittingDispute}
                />
              </div>

              <div>
                <label className="block text-[10px] text-white/40 uppercase tracking-widest mb-1.5 font-bold">Mô Tả Chi Tiết</label>
                <textarea
                  value={disputeDesc}
                  onChange={(e) => setDisputeDesc(e.target.value)}
                  placeholder="Nhập chi tiết các buổi tập lỗi hoặc hành vi sai phạm của HLV..."
                  required
                  rows={4}
                  className="input-dark focus:border-brand-red py-2"
                  disabled={submittingDispute}
                />
              </div>

              <div>
                <label className="block text-[10px] text-white/40 uppercase tracking-widest mb-1.5 font-bold">Ảnh Minh Chứng (Nếu có)</label>
                <input
                  type="file"
                  accept="image/*"
                  onChange={(e) => setDisputeFile(e.target.files?.[0] || null)}
                  className="input-dark py-2"
                  disabled={submittingDispute}
                />
              </div>

              <div className="flex gap-3 justify-end mt-4">
                <button
                  type="button"
                  onClick={() => setShowDisputeModal(false)}
                  className="px-4 py-2 border border-brand-border text-white/60 hover:text-white text-xs font-semibold uppercase tracking-wider cursor-pointer transition-colors"
                  disabled={submittingDispute}
                >
                  HỦY
                </button>
                <button
                  type="submit"
                  disabled={submittingDispute}
                  className="btn-primary py-2 px-4 text-xs tracking-wider justify-center min-w-[120px]"
                >
                  {submittingDispute ? (
                    <div className="flex items-center gap-1.5">
                      <Loader2 className="animate-spin text-white" size={12} />
                      ĐANG GỬI...
                    </div>
                  ) : (
                    'GỬI KHIẾU NẠI'
                  )}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
};

export default WorkspacePage;
