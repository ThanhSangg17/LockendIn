// src/context/DataContext.tsx
import React, { createContext, useContext, useState, useEffect } from 'react';
import { useAuth } from './AuthContext';
import api from '../services/api';
import { 
  saveMockWorkspaces, 
  getMockWorkspaces,
  getMockMessages, 
  saveMockMessages, 
  simulatePTResponse 
} from '../services/mockFirebase';
import type { Workspace, ChatMessage } from '../services/mockFirebase';
import { 
  getMockMealPlans, 
  generateAIMealPlan 
} from '../services/mockGemini';
import type { MealPlan } from '../services/mockGemini';

export interface PTPackage {
  id: string;
  ptId: string;
  ptName: string;
  name: string;
  description: string;
  sessionsCount: number;
  price: number; // VND
  isActive: boolean;
}

export interface Booking {
  id: string;
  customerId: string;
  customerName: string;
  ptId: string;
  ptName: string;
  packageId: string;
  packageName: string;
  sessionsCount: number;
  price: number;
  status: 'Unpaid' | 'PaidPendingAcceptance' | 'Active' | 'Completed' | 'Cancelled';
  paymentTxId?: string;
  createdAt: string;
}

export interface Review {
  id: string;
  ptId: string;
  customerId: string;
  customerName: string;
  rating: number; // 1-5
  comment: string;
  createdAt: string;
}

export interface Dispute {
  id: string;
  bookingId: string;
  workspaceId: string;
  customerId: string;
  customerName: string;
  ptId: string;
  ptName: string;
  reason: string;
  description: string;
  evidenceImage?: string;
  status: 'Pending' | 'ResolvedRefunded' | 'ResolvedPaidPT';
  createdAt: string;
}

export interface Settlement {
  id: string;
  ptId: string;
  ptName: string;
  bookingId: string;
  amount: number;
  status: 'PendingDisputeWindow' | 'Released' | 'LockedByDispute';
  createdAt: string;
  releasedAt?: string;
}

export interface AuditLog {
  id: string;
  actor: string;
  action: string;
  details: string;
  timestamp: string;
}

export interface AppNotification {
  id: string;
  recipientId: string; // user ID or 'admin'
  title: string;
  message: string;
  type: 'info' | 'success' | 'warning';
  read: boolean;
  createdAt: string;
}

export interface PTVerification {
  id: string;
  userId: string;
  fullName: string;
  bio: string;
  specialization: string;
  experienceYears: number;
  verificationStatus: number;
  averageRating: number;
  totalReviews: number;
}

interface DataContextType {
  packages: PTPackage[];
  bookings: Booking[];
  workspaces: Workspace[];
  chatMessages: ChatMessage[];
  mealPlans: MealPlan[];
  reviews: Review[];
  disputes: Dispute[];
  settlements: Settlement[];
  auditLogs: AuditLog[];
  notifications: AppNotification[];
  transactions: any[];
  ptVerifications: PTVerification[];
  
  // Package methods
  createPackage: (name: string, description: string, sessions: number, price: number) => void;
  updatePackage: (pkgId: string, name: string, description: string, sessions: number, price: number, active: boolean) => void;
  
  // Booking & PayOS simulation methods
  createBooking: (pkg: PTPackage) => Promise<Booking>;
  cancelBooking: (bookingId: string) => void;
  simulatePayOSPayment: (bookingId: string) => Promise<void>;
  acceptBooking: (bookingId: string) => void;
  rejectBooking: (bookingId: string) => void;
  completeCourse: (workspaceId: string) => void;

  // Workspace & Chat
  sendChatMessage: (workspaceId: string, text: string, image?: string) => void;
  saveWorkspaceNotes: (workspaceId: string, notes: string) => void;
  incrementWorkspaceSession: (workspaceId: string) => void;
  
  // AI Meal Plan
  triggerMealPlanGeneration: (workspaceId: string, stats: any) => Promise<MealPlan>;

  // Review & Dispute
  addReview: (ptId: string, rating: number, comment: string) => void;
  fileDispute: (workspaceId: string, reason: string, description: string, file?: File) => Promise<void>;
  resolveDisputeAction: (disputeId: string, decision: 'refund' | 'pay_pt') => void;

  // Admin and Payouts
  approvePTVerification: (ptId: string) => void;
  rejectPTVerification: (ptId: string) => void;
  requestAdditionalDocs: (ptId: string) => void;
  blockPT: (ptId: string) => void;
  releaseSettlement: (settlementId: string) => void;
  
  // Utilities
  markNotificationsAsRead: () => void;
  markNotificationAsRead: (id: string) => Promise<void>;
  refreshBackendData: () => Promise<void>;
  refreshDisputes: () => Promise<void>;
}

const DataContext = createContext<DataContextType | undefined>(undefined);

export const DataProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const { currentUser, refreshUsersList } = useAuth();
  
  // App States
  const [packages, setPackages] = useState<PTPackage[]>([]);
  const [bookings, setBookings] = useState<Booking[]>([]);
  const [workspaces, setWorkspaces] = useState<Workspace[]>([]);
  const [chatMessages, setChatMessages] = useState<ChatMessage[]>([]);
  const [mealPlans, setMealPlans] = useState<MealPlan[]>([]);
  const [reviews] = useState<Review[]>([]);
  const [disputes, setDisputes] = useState<Dispute[]>([]);
  const [settlements, setSettlements] = useState<Settlement[]>([]);
  const [auditLogs, setAuditLogs] = useState<AuditLog[]>([]);
  const [notifications, setNotifications] = useState<AppNotification[]>([]);
  const [transactions] = useState<any[]>([]);
  const [ptVerifications, setPtVerifications] = useState<PTVerification[]>([]);

  const mapBookingStatus = (statusNum: number): 'Unpaid' | 'PaidPendingAcceptance' | 'Active' | 'Completed' | 'Cancelled' => {
    switch (statusNum) {
      case 1: return 'Unpaid';
      case 2: return 'PaidPendingAcceptance';
      case 3: return 'Active';
      case 4:
      case 5: return 'Completed';
      case 6:
      case 7: return 'Cancelled';
      default: return 'Unpaid';
    }
  };

  const fetchBackendData = async () => {
    if (!currentUser) return;
    
    // 1. Fetch Packages
    try {
      if (currentUser.role === 'pt') {
        const res = await api.get('/packages/my');
        if (res.data?.success) {
          const items = res.data.data.map((p: any) => ({
            id: p.id,
            ptId: p.ptProfileId,
            ptName: currentUser.fullName,
            name: p.name,
            description: p.description,
            sessionsCount: p.sessionCount,
            price: p.price,
            isActive: p.isActive
          }));
          if (items.length === 0) {
            const defaultPkgs = [
              {
                id: 'pkg-default-1',
                ptId: currentUser.id,
                ptName: currentUser.fullName,
                name: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
                description: 'Lộ trình huấn luyện chuyên sâu 1-kèm-1 trong 12 buổi, tập trung vào các kỹ thuật nâng tạ chuẩn, dinh dưỡng tăng cơ và tối ưu hóa phục hồi.',
                sessionsCount: 12,
                price: 3600000,
                isActive: true
              },
              {
                id: 'pkg-default-2',
                ptId: currentUser.id,
                ptName: currentUser.fullName,
                name: 'Giảm Mỡ & Cardio Đốt Calo',
                description: 'Chương trình cardio cường độ cao (HIIT) kết hợp tạ kháng lực nhẹ giúp đốt mỡ thừa tối ưu trong 24 buổi, kèm theo thực đơn dinh dưỡng chi tiết.',
                sessionsCount: 24,
                price: 6800000,
                isActive: true
              }
            ];
            setPackages(defaultPkgs);
          } else {
            setPackages(items);
          }
        } else {
          throw new Error('packages failed');
        }
      } else {
        const ptsRes = await api.get('/marketplace/pts');
        if (ptsRes.data?.success) {
          const pts = ptsRes.data.data.items || [];
          const allPkgs: PTPackage[] = [];
          for (const pt of pts) {
            try {
              const pkgRes = await api.get(`/marketplace/pts/${pt.ptProfileId}/packages`);
              if (pkgRes.data?.success) {
                allPkgs.push(...pkgRes.data.data.map((p: any) => ({
                  id: p.id,
                  ptId: p.ptProfileId,
                  ptName: pt.fullName,
                  name: p.name,
                  description: p.description,
                  sessionsCount: p.sessionCount,
                  price: p.price,
                  isActive: p.isActive
                })));
              }
            } catch (e) {
              console.warn("Failed to load packages for pt:", pt.ptProfileId, e);
            }
          }
          if (allPkgs.length === 0) {
            const mockPkgs = [
              {
                id: 'pkg-mock-1',
                ptId: 'pt-mock-1',
                ptName: 'PT Test Account',
                name: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
                description: 'Lộ trình huấn luyện chuyên sâu 1-kèm-1 trong 12 buổi, tập trung vào các kỹ thuật nâng tạ chuẩn, dinh dưỡng tăng cơ và tối ưu hóa phục hồi.',
                sessionsCount: 12,
                price: 3600000,
                isActive: true
              },
              {
                id: 'pkg-mock-2',
                ptId: 'pt-mock-2',
                ptName: 'Trần Thị Lan',
                name: 'Giảm Mỡ & Cardio Đốt Calo',
                description: 'Chương trình cardio cường độ cao (HIIT) kết hợp tạ kháng lực nhẹ giúp đốt mỡ thừa tối ưu trong 24 buổi, kèm theo thực đơn dinh dưỡng chi tiết.',
                sessionsCount: 24,
                price: 6800000,
                isActive: true
              }
            ];
            setPackages(mockPkgs);
          } else {
            setPackages(allPkgs);
          }
        } else {
          throw new Error('marketplace failed');
        }
      }
    } catch (e) {
      console.warn("Failed to fetch packages, loading mock data:", e);
      const mockPkgs = [
        {
          id: 'pkg-mock-1',
          ptId: 'pt-mock-1',
          ptName: 'PT Test Account',
          name: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
          description: 'Lộ trình huấn luyện chuyên sâu 1-kèm-1 trong 12 buổi, tập trung vào các kỹ thuật nâng tạ chuẩn, dinh dưỡng tăng cơ và tối ưu hóa phục hồi.',
          sessionsCount: 12,
          price: 3600000,
          isActive: true
        },
        {
          id: 'pkg-mock-2',
          ptId: 'pt-mock-2',
          ptName: 'Trần Thị Lan',
          name: 'Giảm Mỡ & Cardio Đốt Calo',
          description: 'Chương trình cardio cường độ cao (HIIT) kết hợp tạ kháng lực nhẹ giúp đốt mỡ thừa tối ưu trong 24 buổi, kèm theo thực đơn dinh dưỡng chi tiết.',
          sessionsCount: 24,
          price: 6800000,
          isActive: true
        }
      ];
      setPackages(mockPkgs);
    }

    // 2. Fetch Bookings
    let activeBookings: Booking[] = [];
    
    const getPtMockBookings = (ptId: string, ptName: string): Booking[] => [
      {
        id: 'booking-mock-pending',
        customerId: 'cust-mock-1',
        customerName: 'Nguyễn Văn A',
        ptId: ptId,
        ptName: ptName,
        packageId: 'pkg-default-1',
        packageName: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
        sessionsCount: 12,
        price: 3600000,
        status: 'PaidPendingAcceptance',
        createdAt: new Date(Date.now() - 3600 * 1000).toISOString()
      },
      {
        id: 'booking-mock-active',
        customerId: 'cust-mock-2',
        customerName: 'Lê Thị B',
        ptId: ptId,
        ptName: ptName,
        packageId: 'pkg-default-2',
        packageName: 'Giảm Mỡ & Cardio Đốt Calo',
        sessionsCount: 24,
        price: 6800000,
        status: 'Active',
        createdAt: new Date(Date.now() - 24 * 3600 * 1000).toISOString()
      },
      {
        id: 'booking-mock-active-2',
        customerId: 'cust-mock-6',
        customerName: 'Trần Văn Hùng',
        ptId: ptId,
        ptName: ptName,
        packageId: 'pkg-default-1',
        packageName: 'Cải Thiện Tư Thế & Đàn Hồi',
        sessionsCount: 12,
        price: 3600000,
        status: 'Active',
        createdAt: new Date(Date.now() - 5 * 24 * 3600 * 1000).toISOString()
      },
      {
        id: 'booking-mock-completed',
        customerId: 'cust-mock-3',
        customerName: 'Trần Đức C',
        ptId: ptId,
        ptName: ptName,
        packageId: 'pkg-default-1',
        packageName: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
        sessionsCount: 12,
        price: 3600000,
        status: 'Completed',
        createdAt: new Date(Date.now() - 10 * 24 * 3600 * 1000).toISOString()
      },
      {
        id: 'booking-mock-unpaid',
        customerId: 'cust-mock-4',
        customerName: 'Phạm Quốc Đạt',
        ptId: ptId,
        ptName: ptName,
        packageId: 'pkg-default-1',
        packageName: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
        sessionsCount: 12,
        price: 3600000,
        status: 'Unpaid',
        createdAt: new Date(Date.now() - 2 * 3600 * 1000).toISOString()
      },
      {
        id: 'booking-mock-cancelled',
        customerId: 'cust-mock-5',
        customerName: 'Lê Hoàng Long',
        ptId: ptId,
        ptName: ptName,
        packageId: 'pkg-default-2',
        packageName: 'Giảm Mỡ & Cardio Đốt Calo',
        sessionsCount: 24,
        price: 6800000,
        status: 'Cancelled',
        createdAt: new Date(Date.now() - 3 * 24 * 3600 * 1000).toISOString()
      }
    ];

    const getCustMockBookings = (custId: string, custName: string): Booking[] => [
      {
        id: 'booking-mock-active',
        customerId: custId,
        customerName: custName,
        ptId: 'pt-mock-1',
        ptName: 'PT Test Account',
        packageId: 'pkg-mock-1',
        packageName: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
        sessionsCount: 12,
        price: 3600000,
        status: 'Active',
        createdAt: new Date(Date.now() - 24 * 3600 * 1000).toISOString()
      },
      {
        id: 'booking-mock-active-2',
        customerId: custId,
        customerName: custName,
        ptId: 'pt-mock-2',
        ptName: 'Trần Thị Lan',
        packageId: 'pkg-mock-2',
        packageName: 'Giảm Mỡ & Cardio Đốt Calo',
        sessionsCount: 24,
        price: 6800000,
        status: 'Active',
        createdAt: new Date(Date.now() - 5 * 24 * 3600 * 1000).toISOString()
      },
      {
        id: 'booking-mock-completed',
        customerId: custId,
        customerName: custName,
        ptId: 'pt-mock-1',
        ptName: 'PT Test Account',
        packageId: 'pkg-mock-1',
        packageName: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
        sessionsCount: 12,
        price: 3600000,
        status: 'Completed',
        createdAt: new Date(Date.now() - 10 * 24 * 3600 * 1000).toISOString()
      },
      {
        id: 'booking-mock-unpaid',
        customerId: custId,
        customerName: custName,
        ptId: 'pt-mock-2',
        ptName: 'Trần Thị Lan',
        packageId: 'pkg-mock-2',
        packageName: 'Giảm Mỡ & Cardio Đốt Calo',
        sessionsCount: 24,
        price: 6800000,
        status: 'Unpaid',
        createdAt: new Date(Date.now() - 3600 * 1000).toISOString()
      }
    ];

    let mergedBookings: Booking[] = [];

    try {
      const res = await api.get('/bookings/my');
      if (res.data?.success) {
        const mapped = res.data.data.map((b: any) => {
          return {
            id: b.id,
            customerId: b.customerId,
            customerName: b.customerId === currentUser.id ? currentUser.fullName : 'Học Viên',
            ptId: b.ptProfileId,
            ptName: b.ptProfileId === currentUser.id ? currentUser.fullName : 'Huấn Luyện Viên',
            packageId: b.packageId,
            packageName: 'Gói Tập Luyện',
            sessionsCount: b.sessionCount,
            price: b.totalAmount,
            status: mapBookingStatus(b.status),
            createdAt: b.createdAt
          };
        });
        
        mergedBookings = [...mapped];
        const mocks = currentUser.role === 'pt' 
          ? getPtMockBookings(currentUser.id, currentUser.fullName) 
          : getCustMockBookings(currentUser.id, currentUser.fullName);
        
        mocks.forEach(mockB => {
          if (!mergedBookings.some(b => b.id === mockB.id)) {
            mergedBookings.push(mockB);
          }
        });
        
        setBookings(mergedBookings);
        activeBookings = mergedBookings.filter(b => b.status === 'Active');
      } else {
        throw new Error('bookings failed');
      }
    } catch (e) {
      console.warn('Failed to fetch bookings, loading mock:', e);
      const mocks = currentUser.role === 'pt' 
        ? getPtMockBookings(currentUser.id, currentUser.fullName) 
        : getCustMockBookings(currentUser.id, currentUser.fullName);
      mergedBookings = mocks;
      setBookings(mocks);
      activeBookings = mocks.filter(b => b.status === 'Active');
    }

    // 3. Fetch Workspaces for Active Bookings
    const list: Workspace[] = [];
    
    // Add completed mock workspace if completed mock booking is present
    const completedMock = mergedBookings.find(b => b.id === 'booking-mock-completed');
    if (completedMock) {
      list.push({
        id: 'ws-mock-completed',
        bookingId: 'booking-mock-completed',
        customerId: completedMock.customerId,
        customerName: completedMock.customerName,
        ptId: completedMock.ptId,
        ptName: completedMock.ptName,
        packageName: completedMock.packageName,
        sessionsTotal: completedMock.sessionsCount,
        sessionsCompleted: completedMock.sessionsCount,
        status: 'completed',
        ptNotes: 'Khóa học đã hoàn thành xuất sắc. Học viên đạt mục tiêu tăng 3kg cơ và giảm mỡ tốt.',
        createdAt: completedMock.createdAt
      });
    }

    if (activeBookings.length > 0) {
      const savedMockWorkspaces = getMockWorkspaces();
      for (const b of activeBookings) {
        if (b.id.startsWith('booking-mock')) {
          const targetId = b.id === 'booking-mock-active' ? 'ws-mock-active' : b.id === 'booking-mock-active-2' ? 'ws-mock-active-2' : `ws-${b.id}`;
          const existingMock = savedMockWorkspaces.find(mw => mw.id === targetId);
          list.push({
            id: targetId,
            bookingId: b.id,
            customerId: b.customerId,
            customerName: b.customerName,
            ptId: b.ptId,
            ptName: b.ptName,
            packageName: b.packageName,
            sessionsTotal: b.sessionsCount,
            sessionsCompleted: existingMock ? existingMock.sessionsCompleted : (b.id === 'booking-mock-active' ? 4 : b.id === 'booking-mock-active-2' ? 8 : 0),
            status: existingMock ? existingMock.status : 'active',
            ptNotes: existingMock?.ptNotes || (currentUser.role === 'pt'
              ? 'Học viên thể lực tốt, đẩy ngực đều. Nên tăng thêm bài tập đùi đĩa để phát triển toàn diện.'
              : 'Hãy ăn đủ đạm và giữ chế độ sinh hoạt đều đặn để phục hồi cơ bắp tốt nhất nhé.'),
            createdAt: b.createdAt
          });
          continue;
        }

        try {
          const res = await api.get(`/workspaces/booking/${b.id}`);
          if (res.data?.success) {
            const w = res.data.data;
            const existingMock = savedMockWorkspaces.find(mw => mw.id === w.id);
            list.push({
              id: w.id,
              bookingId: w.bookingId,
              customerId: w.customerId,
              customerName: b.customerName,
              ptId: w.ptProfileId,
              ptName: b.ptName,
              packageName: b.packageName,
              sessionsTotal: b.sessionsCount,
              sessionsCompleted: w.status === 2 ? b.sessionsCount : (existingMock ? existingMock.sessionsCompleted : w.sessionCompletedCount || 0),
              status: w.status === 2 ? 'completed' : 'active',
              ptNotes: w.courseNote || '',
              createdAt: w.createdAt
            });
          }
        } catch (e) {
          console.warn(`No workspace found for booking ${b.id}:`, e);
          const targetId = `ws-${b.id}`;
          const existingMock = savedMockWorkspaces.find(mw => mw.id === targetId);
          // Fallback workspace for real active booking if API fails
          list.push({
            id: targetId,
            bookingId: b.id,
            customerId: b.customerId,
            customerName: b.customerName,
            ptId: b.ptId,
            ptName: b.ptName,
            packageName: b.packageName,
            sessionsTotal: b.sessionsCount,
            sessionsCompleted: existingMock ? existingMock.sessionsCompleted : 0,
            status: 'active',
            ptNotes: 'Không tải được ghi chú từ máy chủ.',
            createdAt: b.createdAt
          });
        }
      }
    }
    
    if (list.length === 0) {
      const savedMockWorkspaces = getMockWorkspaces();
      const existingMock = savedMockWorkspaces.find(mw => mw.id === 'ws-mock-active');
      list.push({
        id: 'ws-mock-active',
        bookingId: 'booking-mock-active',
        customerId: currentUser.role === 'pt' ? 'cust-mock-2' : currentUser.id,
        customerName: currentUser.role === 'pt' ? 'Lê Thị B' : currentUser.fullName || 'Học Viên',
        ptId: currentUser.role === 'pt' ? currentUser.id : 'pt-mock-1',
        ptName: currentUser.role === 'pt' ? currentUser.fullName || 'PT Test Account' : 'PT Test Account',
        packageName: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
        sessionsTotal: 12,
        sessionsCompleted: existingMock ? existingMock.sessionsCompleted : 4,
        status: existingMock ? existingMock.status : 'active',
        ptNotes: existingMock?.ptNotes || (currentUser.role === 'pt'
          ? 'Học viên thể lực tốt, đẩy ngực đều. Nên tăng thêm bài tập đùi đĩa để phát triển toàn diện.'
          : 'Hãy ăn đủ đạm và giữ chế độ sinh hoạt đều đặn để phục hồi cơ bắp tốt nhất nhé.'),
        createdAt: new Date().toISOString()
      });
    }
    setWorkspaces(list);

    // 4. Fetch Disputes
    await refreshDisputes();

    // 5. Fetch Notifications
    try {
      const res = await api.get('/notifications/my');
      if (res.data?.success) {
        const mapped = res.data.data.map((n: any) => ({
          id: n.id,
          recipientId: n.userId,
          title: n.title,
          message: n.content,
          type: n.type === 2 ? 'warning' : 'success',
          read: n.isRead,
          createdAt: n.createdAt
        }));
        if (mapped.length === 0) {
          const mockNotifications: AppNotification[] = [
            {
              id: 'notif-mock-1',
              recipientId: currentUser.id,
              title: 'Thanh Toán Thành Công',
              message: 'Chúc mừng! Gói tập luyện của bạn đã được thanh toán escrow an toàn và lớp học đã sẵn sàng.',
              type: 'success',
              read: false,
              createdAt: new Date(Date.now() - 3600 * 1000).toISOString()
            }
          ];
          setNotifications(mockNotifications);
        } else {
          setNotifications(mapped);
        }
      } else {
        throw new Error('notifications failed');
      }
    } catch (e) {
      console.warn('Failed to fetch notifications, loading mock:', e);
      const mockNotifications: AppNotification[] = [
        {
          id: 'notif-mock-1',
          recipientId: currentUser.id,
          title: 'Thanh Toán Thành Công',
          message: 'Chúc mừng! Gói tập luyện của bạn đã được thanh toán escrow an toàn và lớp học đã sẵn sàng.',
          type: 'success',
          read: false,
          createdAt: new Date(Date.now() - 3600 * 1000).toISOString()
        }
      ];
      setNotifications(mockNotifications);
    }

    // 6. Admin Panel Fetch
    if (currentUser.role === 'admin') {
      try {
        const setRes = await api.get('/admin/settlements');
        if (setRes.data?.success) {
          const mapped = setRes.data.data.map((s: any) => ({
            id: s.id,
            ptId: s.ptProfileId,
            ptName: 'Huấn Luyện Viên',
            bookingId: s.bookingId,
            amount: s.netAmount,
            status: s.status === 1 ? 'PendingDisputeWindow' : 'Released',
            createdAt: s.createdAt
          }));
          if (mapped.length === 0) {
            const mockSettlements: Settlement[] = [
              {
                id: 'set-mock-1',
                ptId: 'pt-mock-1',
                ptName: 'PT Test Account',
                bookingId: 'booking-mock-completed',
                amount: 3240000,
                status: 'PendingDisputeWindow',
                createdAt: new Date(Date.now() - 3600 * 1000).toISOString()
              },
              {
                id: 'set-mock-2',
                ptId: 'pt-mock-2',
                ptName: 'Trần Thị Lan',
                bookingId: 'booking-mock-completed',
                amount: 6120000,
                status: 'Released',
                createdAt: new Date(Date.now() - 5 * 24 * 3600 * 1000).toISOString(),
                releasedAt: new Date(Date.now() - 2 * 24 * 3600 * 1000).toISOString()
              }
            ];
            setSettlements(mockSettlements);
          } else {
            setSettlements(mapped);
          }
        } else {
          throw new Error('settlements failed');
        }
      } catch (e) {
        console.warn('Failed to fetch settlements, loading mock:', e);
        const mockSettlements: Settlement[] = [
          {
            id: 'set-mock-1',
            ptId: 'pt-mock-1',
            ptName: 'PT Test Account',
            bookingId: 'booking-mock-completed',
            amount: 3240000,
            status: 'PendingDisputeWindow',
            createdAt: new Date(Date.now() - 3600 * 1000).toISOString()
          },
          {
            id: 'set-mock-2',
            ptId: 'pt-mock-2',
            ptName: 'Trần Thị Lan',
            bookingId: 'booking-mock-completed',
            amount: 6120000,
            status: 'Released',
            createdAt: new Date(Date.now() - 5 * 24 * 3600 * 1000).toISOString(),
            releasedAt: new Date(Date.now() - 2 * 24 * 3600 * 1000).toISOString()
          }
        ];
        setSettlements(mockSettlements);
      }

      try {
        const auditRes = await api.get('/admin/audit-logs');
        if (auditRes.data?.success) {
          const mapped = auditRes.data.data.map((l: any) => ({
            id: l.id,
            actor: l.actorUserId,
            action: l.action,
            details: l.metadataJson || '',
            timestamp: l.createdAt
          }));
          if (mapped.length === 0) {
            const mockLogs: AuditLog[] = [
              {
                id: 'log-mock-1',
                actor: 'admin@lockedin.vn',
                action: 'RESOLVE_DISPUTE',
                details: 'Giải quyết khiếu nại disp-mock-2 (Hoàn tiền học viên 100%)',
                timestamp: new Date(Date.now() - 4 * 24 * 3600 * 1000).toISOString()
              },
              {
                id: 'log-mock-2',
                actor: 'admin@lockedin.vn',
                action: 'APPROVE_PT',
                details: 'Phê duyệt HLV PT Test Account hiển thị trên hệ thống',
                timestamp: new Date(Date.now() - 15 * 24 * 3600 * 1000).toISOString()
              }
            ];
            setAuditLogs(mockLogs);
          } else {
            setAuditLogs(mapped);
          }
        } else {
          throw new Error('audit-logs failed');
        }
      } catch (e) {
        console.warn('Failed to fetch audit logs, loading mock:', e);
        const mockLogs: AuditLog[] = [
          {
            id: 'log-mock-1',
            actor: 'admin@lockedin.vn',
            action: 'RESOLVE_DISPUTE',
            details: 'Giải quyết khiếu nại disp-mock-2 (Hoàn tiền học viên 100%)',
            timestamp: new Date(Date.now() - 4 * 24 * 3600 * 1000).toISOString()
          },
          {
            id: 'log-mock-2',
            actor: 'admin@lockedin.vn',
            action: 'APPROVE_PT',
            details: 'Phê duyệt HLV PT Test Account hiển thị trên hệ thống',
            timestamp: new Date(Date.now() - 15 * 24 * 3600 * 1000).toISOString()
          }
        ];
        setAuditLogs(mockLogs);
      }

      try {
        const ptRes = await api.get('/admin/pt-verifications');
        if (ptRes.data?.success) {
          const list = ptRes.data.data || [];
          if (list.length === 0) {
            const mockVerifications: PTVerification[] = [
              {
                id: 'ptv-mock-1',
                userId: 'pt-mock-1',
                fullName: 'PT Test Account',
                bio: 'Chuyên gia thể hình với hơn 8 năm kinh nghiệm trong lĩnh vực nâng tạ, tăng cơ và phục hồi chấn thương.',
                specialization: 'Tăng Cơ, Phục Hồi Chấn Thương',
                experienceYears: 8,
                verificationStatus: 1, // Pending
                averageRating: 0.0,
                totalReviews: 0
              }
            ];
            setPtVerifications(mockVerifications);
          } else {
            setPtVerifications(list);
          }
        } else {
          throw new Error('pt-verifications failed');
        }
      } catch (e) {
        console.error('Failed to fetch pt verifications, loading mock:', e);
        const mockVerifications: PTVerification[] = [
          {
            id: 'ptv-mock-1',
            userId: 'pt-mock-1',
            fullName: 'PT Test Account',
            bio: 'Chuyên gia thể hình với hơn 8 năm kinh nghiệm trong lĩnh vực nâng tạ, tăng cơ và phục hồi chấn thương.',
            specialization: 'Tăng Cơ, Phục Hồi Chấn Thương',
            experienceYears: 8,
            verificationStatus: 1, // Pending
            averageRating: 0.0,
            totalReviews: 0
          }
        ];
        setPtVerifications(mockVerifications);
      }
    }
  };

  // Sync state with backend on mount & login user change
  useEffect(() => {
    if (currentUser) {
      fetchBackendData();
    } else {
      // Clear state on logout
      setPackages([]);
      setBookings([]);
      setWorkspaces([]);
      setDisputes([]);
      setSettlements([]);
      setNotifications([]);
      setPtVerifications([]);
    }
  }, [currentUser]);

  // Universal state saver helper
  const syncState = (key: string, data: any, stateSetter: any) => {
    localStorage.setItem(key, JSON.stringify(data));
    stateSetter(data);
  };

  // Add notification helper
  const addNotification = (recipientId: string, title: string, message: string, type: 'info' | 'success' | 'warning' = 'info') => {
    const list: AppNotification[] = JSON.parse(localStorage.getItem('lockedin_notifications') || '[]');
    const newNotif: AppNotification = {
      id: 'notif-' + Math.random().toString(36).substring(2, 9),
      recipientId,
      title,
      message,
      type,
      read: false,
      createdAt: new Date().toISOString()
    };
    list.unshift(newNotif);
    syncState('lockedin_notifications', list, setNotifications);
  };

  const incrementWorkspaceSession = async (workspaceId: string) => {
    try {
      if (workspaceId.startsWith('ws-mock')) {
        // Fallback for mock workspaces
        const mw = getMockWorkspaces();
        const existing = mw.find(w => w.id === workspaceId);
        if (existing && existing.sessionsCompleted < existing.sessionsTotal) {
          existing.sessionsCompleted++;
          if (existing.sessionsCompleted === existing.sessionsTotal) {
            existing.status = 'completed';
          }
          saveMockWorkspaces(mw);
        }
        setWorkspaces(prev => prev.map(w => {
          if (w.id === workspaceId && w.sessionsCompleted < w.sessionsTotal) {
            const nextCompleted = w.sessionsCompleted + 1;
            return {
              ...w,
              sessionsCompleted: nextCompleted,
              status: nextCompleted === w.sessionsTotal ? 'completed' as const : w.status
            };
          }
          return w;
        }));
        return;
      }

      // Call the real backend API
      const res = await api.post(`/workspaces/${workspaceId}/sessions`);
      if (res.data?.success) {
        // Update local state optimistic UI or fetch data again
        setWorkspaces(prev => prev.map(w => {
          if (w.id === workspaceId && w.sessionsCompleted < w.sessionsTotal) {
            const nextCompleted = w.sessionsCompleted + 1;
            return {
              ...w,
              sessionsCompleted: nextCompleted,
              status: nextCompleted === w.sessionsTotal ? 'completed' as const : w.status
            };
          }
          return w;
        }));
        
        // Also update the associated booking if it completed
        const updatedWs = res.data.data;
        if (updatedWs.status === 2) {
          setBookings(prev => prev.map(b => {
            if (b.id === updatedWs.bookingId) {
              return { ...b, status: 'Completed' };
            }
            return b;
          }));
        }
      } else {
        console.error('Failed to log session:', res.data?.message);
        throw new Error(res.data?.message || 'Failed to log session');
      }
    } catch (e) {
      console.error('Failed to increment session count', e);
      throw e;
    }
  };

  // Add audit log helper
  const addAuditLog = (actor: string, action: string, details: string) => {
    const list: AuditLog[] = JSON.parse(localStorage.getItem('lockedin_audit_logs') || '[]');
    const newLog: AuditLog = {
      id: 'log-' + Math.random().toString(36).substring(2, 9),
      actor,
      action,
      details,
      timestamp: new Date().toISOString()
    };
    list.unshift(newLog);
    syncState('lockedin_audit_logs', list.slice(0, 150), setAuditLogs); // Cap logs at 150
  };

  // ==========================================
  // PACKAGE METHODS
  // ==========================================
  
  const createPackage = async (name: string, description: string, sessions: number, price: number) => {
    if (!currentUser || currentUser.role !== 'pt') return;
    try {
      const res = await api.post('/packages', {
        name,
        description,
        sessionCount: sessions,
        price
      });
      if (res.data?.success) {
        fetchBackendData();
      }
    } catch (e) {
      console.error('Failed to create package:', e);
    }
  };

  const updatePackage = async (pkgId: string, name: string, description: string, sessions: number, price: number, active: boolean) => {
    if (!currentUser) return;
    try {
      const res = await api.put(`/packages/${pkgId}`, {
        name,
        description,
        sessionCount: sessions,
        price
      });
      if (res.data?.success) {
        const currentPkg = packages.find(p => p.id === pkgId);
        if (currentPkg && currentPkg.isActive !== active) {
          await api.patch(`/packages/${pkgId}/${active ? 'show' : 'hide'}`);
        }
        fetchBackendData();
      }
    } catch (e) {
      console.error('Failed to update package:', e);
    }
  };

  // ==========================================
  // BOOKING & PAYOS INTEGRATION SIMULATION
  // ==========================================

  const createBooking = async (pkg: PTPackage): Promise<Booking> => {
    if (!currentUser) throw new Error('Must be logged in to create bookings');
    
    // Check if it's a mock package ID (e.g., does not match GUID pattern or contains mock/default)
    const isGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(pkg.id);
    if (!isGuid || pkg.id.includes('mock') || pkg.id.includes('default') || pkg.id.startsWith('pkg-')) {
      const mockBookingId = 'booking-mock-' + Math.random().toString(36).substring(2, 9);
      const newBooking: Booking = {
        id: mockBookingId,
        customerId: currentUser.id,
        customerName: currentUser.fullName,
        ptId: pkg.ptId,
        ptName: pkg.ptName,
        packageId: pkg.id,
        packageName: pkg.name,
        sessionsCount: pkg.sessionsCount,
        price: pkg.price,
        status: 'Unpaid',
        createdAt: new Date().toISOString()
      };
      
      // Save to local bookings state
      setBookings(prev => [newBooking, ...prev]);
      return newBooking;
    }

    try {
      const res = await api.post('/bookings', {
        packageId: pkg.id
      });
      if (res.data?.success) {
        const b = res.data.data;
        const newBooking: Booking = {
          id: b.id,
          customerId: b.customerId,
          customerName: currentUser.fullName,
          ptId: b.ptProfileId,
          ptName: pkg.ptName,
          packageId: b.packageId,
          packageName: pkg.name,
          sessionsCount: b.sessionCount,
          price: b.totalAmount,
          status: 'Unpaid',
          createdAt: b.createdAt
        };
        fetchBackendData();
        return newBooking;
      }
      throw new Error(res.data?.message || 'Failed to create booking');
    } catch (e: any) {
      console.error('Failed to create booking:', e);
      throw e;
    }
  };

  const cancelBooking = async (bookingId: string) => {
    try {
      const res = await api.post(`/bookings/${bookingId}/cancel`);
      if (res.data?.success) {
        fetchBackendData();
      }
    } catch (e) {
      console.error('Failed to cancel booking:', e);
    }
  };

  // PayOS Checkout success simulator
  const simulatePayOSPayment = async (bookingId: string) => {
    try {
      const payRes = await api.get(`/payments/booking/${bookingId}`);
      if (payRes.data?.success && payRes.data.data) {
        const payment = payRes.data.data;
        await api.post('/payments/payos/webhook', {
          code: '00',
          desc: 'success',
          data: payment.orderCode,
          signature: 'simulated'
        });
        await fetchBackendData();
      } else {
        throw new Error(payRes.data?.message || 'Payment not found on backend');
      }
    } catch (e) {
      console.error('Failed to simulate PayOS payment via API webhook:', e);
      // Fallback to local UI simulation if backend fails
      setBookings(prev => prev.map(b => b.id === bookingId ? { ...b, status: 'PaidPendingAcceptance' as const } : b));
    }
  };

  const acceptBooking = async (bookingId: string) => {
    if (!currentUser || currentUser.role !== 'pt') return;
    
    if (bookingId.startsWith('booking-mock')) {
      setBookings(prev => prev.map(b => b.id === bookingId ? { ...b, status: 'Active' as const } : b));
      setWorkspaces(prev => {
        if (prev.some(w => w.bookingId === bookingId)) return prev;
        return [
          ...prev,
          {
            id: bookingId === 'booking-mock-pending' ? 'ws-mock-pending' : `ws-${bookingId}`,
            bookingId: bookingId,
            customerId: 'cust-mock-1',
            customerName: 'Nguyễn Văn A',
            ptId: currentUser.id,
            ptName: currentUser.fullName,
            packageName: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
            sessionsTotal: 12,
            sessionsCompleted: 0,
            status: 'active' as const,
            ptNotes: 'Lớp học mới được chấp nhận.',
            createdAt: new Date().toISOString()
          }
        ];
      });
      addNotification(currentUser.id, 'Chấp Nhận Lịch Đặt', 'Bạn đã chấp nhận lịch đặt của Nguyễn Văn A.', 'success');
      return;
    }

    try {
      const res = await api.post(`/bookings/${bookingId}/accept`);
      if (res.data?.success) {
        fetchBackendData();
      }
    } catch (e) {
      console.error('Failed to accept booking:', e);
    }
  };

  const rejectBooking = async (bookingId: string) => {
    if (!currentUser || currentUser.role !== 'pt') return;

    if (bookingId.startsWith('booking-mock')) {
      setBookings(prev => prev.map(b => b.id === bookingId ? { ...b, status: 'Cancelled' as const } : b));
      addNotification(currentUser.id, 'Từ Chối Lịch Đặt', 'Bạn đã từ chối lịch đặt.', 'warning');
      return;
    }

    try {
      const res = await api.post(`/bookings/${bookingId}/reject`);
      if (res.data?.success) {
        fetchBackendData();
      }
    } catch (e) {
      console.error('Failed to reject booking:', e);
    }
  };
  
  const completeCourse = async (workspaceId: string) => {
    if (workspaceId.startsWith('ws-mock') || workspaceId.startsWith('booking-mock')) {
      setWorkspaces(prev => prev.map(w => w.id === workspaceId ? { ...w, status: 'completed' as const } : w));
      const ws = workspaces.find(w => w.id === workspaceId);
      if (ws) {
        setBookings(prev => prev.map(b => b.id === ws.bookingId ? { ...b, status: 'Completed' as const } : b));
      }
      if (currentUser) {
        addNotification(currentUser.id, 'Khóa Học Hoàn Thành', 'Khóa học của bạn đã hoàn thành thành công!', 'success');
      }
      return;
    }

    const ws = workspaces.find(w => w.id === workspaceId);
    if (!ws) return;
    try {
      const res = await api.post(`/bookings/${ws.bookingId}/complete`);
      if (res.data?.success) {
        fetchBackendData();
      }
    } catch (e) {
      console.error('Failed to complete course:', e);
    }
  };

  // ==========================================
  // WORKSPACE & CHAT METHODS
  // ==========================================

  const sendChatMessage = (workspaceId: string, text: string, image?: string) => {
    if (!currentUser) return;

    const newMsg: ChatMessage = {
      id: 'msg-' + Math.random().toString(36).substring(2, 9),
      workspaceId,
      senderId: currentUser.id,
      senderName: currentUser.fullName,
      text,
      image,
      createdAt: new Date().toISOString(),
      read: true
    };

    const updated = [...chatMessages, newMsg];
    saveMockMessages(updated);
    setChatMessages(updated);

    // Check if customer sent it, trigger AI-like simulated response from trainer
    const ws = workspaces.find(w => w.id === workspaceId);
    if (ws && currentUser.role === 'customer') {
      simulatePTResponse(workspaceId, ws.ptId, ws.ptName, text, () => {
        setChatMessages(getMockMessages());
        addNotification(currentUser.id, 'New Chat Message', `Trainer sent you a message in workspace`, 'info');
      });
    } else if (ws && currentUser.role === 'pt') {
      addNotification(ws.customerId, 'New Trainer Message', `Coach ${ws.ptName} sent you a message in workspace`, 'info');
    }
  };

  const saveWorkspaceNotes = async (workspaceId: string, notes: string) => {
    try {
      const res = await api.put(`/workspaces/${workspaceId}/course-note`, {
        courseNote: notes
      });
      if (res.data?.success) {
        setWorkspaces(prev => prev.map(w => w.id === workspaceId ? { ...w, ptNotes: notes } : w));
      }
    } catch (e) {
      console.error('Failed to save workspace notes:', e);
    }
  };


  // ==========================================
  // AI MEAL PLAN GENERATION
  // ==========================================

  const triggerMealPlanGeneration = async (workspaceId: string, stats: any): Promise<MealPlan> => {
    const ws = workspaces.find(w => w.id === workspaceId);
    const trainerName = ws ? ws.ptName : 'Trainer';

    // Call Mock AI Service
    const plan = await generateAIMealPlan(workspaceId, stats);
    
    // Sync local state
    setMealPlans(getMockMealPlans());
    
    if (currentUser) {
      addAuditLog(currentUser.fullName, 'GenerateMealPlan', `Created AI Nutrition Meal Plan via Gemini simulator`);
    }
    
    if (ws) {
      addNotification(ws.customerId, 'New AI Nutrition Plan', `Coach ${trainerName} has generated a new Gemini AI Nutrition Meal Plan for you!`, 'success');
    }

    return plan;
  };

  // ==========================================
  // REVIEW & DISPUTE METHODS
  // ==========================================

  const addReview = async (ptId: string, rating: number, comment: string) => {
    if (!currentUser) return;
    const booking = bookings.find(b => b.ptId === ptId && b.status === 'Completed');
    if (!booking) {
      console.warn("No completed booking found for review.");
      return;
    }
    try {
      const res = await api.post('/reviews', {
        bookingId: booking.id,
        rating,
        comment
      });
      if (res.data?.success) {
        fetchBackendData();
      }
    } catch (e) {
      console.error('Failed to submit review:', e);
    }
  };

  const fileDispute = async (workspaceId: string, reason: string, description: string, file?: File): Promise<void> => {
    if (!currentUser) return;

    const ws = workspaces.find(w => w.id === workspaceId);
    if (!ws) return;

    try {
      const res = await api.post('/disputes', {
        bookingId: ws.bookingId,
        reason,
        description
      });
      if (res.data?.success) {
        const dispute = res.data.data;
        if (file) {
          const formData = new FormData();
          formData.append('file', file);
          const uploadRes = await api.post('/uploads/image?folder=disputes', formData, {
            headers: { 'Content-Type': 'multipart/form-data' }
          });
          if (uploadRes.data?.success) {
            const fileUrl = uploadRes.data.data.secureUrl || uploadRes.data.data.url;
            await api.post(`/disputes/${dispute.id}/evidences`, {
              fileUrl
            });
          }
        }
        fetchBackendData();
      }
    } catch (e) {
      console.error('Failed to raise dispute:', e);
    }
  };

  const resolveDisputeAction = async (disputeId: string, decision: 'refund' | 'pay_pt') => {
    try {
      const endpoint = decision === 'refund' ? 'resolve-refund-customer' : 'resolve-release-to-pt';
      const res = await api.post(`/admin/disputes/${disputeId}/${endpoint}`, {
        resolutionNote: decision === 'refund' ? 'Refunded customer' : 'Released funds to PT'
      });
      if (res.data?.success) {
        fetchBackendData();
      }
    } catch (e) {
      console.error('Failed to resolve dispute action:', e);
    }
  };

  // ==========================================
  // ADMIN PT MANAGEMENT & VERIFICATION
  // ==========================================

  const approvePTVerification = async (ptId: string) => {
    if (ptId.startsWith('ptv-mock-')) {
      setPtVerifications(prev => prev.filter(p => p.id !== ptId));
      return;
    }
    try {
      const res = await api.post(`/admin/pt-verifications/${ptId}/approve`);
      if (res.data?.success) {
        fetchBackendData();
        refreshUsersList();
      }
    } catch (e) {
      console.error('Failed to approve PT verification:', e);
    }
  };

  const rejectPTVerification = async (ptId: string) => {
    if (ptId.startsWith('ptv-mock-')) {
      setPtVerifications(prev => prev.filter(p => p.id !== ptId));
      return;
    }
    try {
      const res = await api.post(`/admin/pt-verifications/${ptId}/reject`);
      if (res.data?.success) {
        fetchBackendData();
        refreshUsersList();
      }
    } catch (e) {
      console.error('Failed to reject PT verification:', e);
    }
  };

  const refreshDisputes = async () => {
    if (!currentUser) return;
    try {
      const endpoint = currentUser.role === 'admin' ? '/admin/disputes' : '/disputes/my';
      const res = await api.get(endpoint);
      if (res.data?.success) {
        const mapped = res.data.data.map((d: any) => ({
          id: d.id,
          bookingId: d.bookingId,
          workspaceId: '',
          customerId: d.customerId,
          customerName: 'Học Viên',
          ptId: d.ptProfileId,
          ptName: 'Huấn Luyện Viên',
          reason: d.reason,
          description: '',
          status: d.status === 1 ? 'Pending' : d.status === 2 ? 'UnderReview' : d.status === 3 ? 'ResolvedRefunded' : 'ResolvedPaidPT',
          createdAt: d.createdAt
        }));
        
        if (mapped.length === 0) {
          const mockDisputes: Dispute[] = [
            {
              id: 'disp-mock-1',
              bookingId: 'booking-mock-completed',
              workspaceId: 'ws-mock-active',
              customerId: 'cust-mock-1',
              customerName: 'Nguyễn Văn A',
              ptId: 'pt-mock-2',
              ptName: 'Trần Thị Lan',
              reason: 'HLV thường xuyên trễ giờ hẹn',
              description: 'HLV Trần Thị Lan đã nghỉ 3 buổi liên tiếp mà không thông báo trước, và khi dạy thì luôn trễ hẹn 15-20 phút. Tôi muốn yêu cầu hoàn trả số tiền còn lại.',
              status: 'Pending',
              createdAt: new Date(Date.now() - 5 * 3600 * 1000).toISOString()
            },
            {
              id: 'disp-mock-2',
              bookingId: 'booking-mock-completed',
              workspaceId: 'ws-mock-active',
              customerId: 'cust-mock-3',
              customerName: 'Trần Đức C',
              ptId: 'pt-mock-1',
              ptName: 'PT Test Account',
              reason: 'Chấn thương trong lúc tập',
              description: 'Bị chấn thương cổ chân do HLV hướng dẫn sai tư thế Squat. Yêu cầu hoàn trả học phí các buổi chưa tập.',
              status: 'ResolvedRefunded',
              createdAt: new Date(Date.now() - 4 * 24 * 3600 * 1000).toISOString()
            }
          ];
          setDisputes(mockDisputes);
        } else {
          setDisputes(mapped);
        }
      } else {
        throw new Error('disputes failed');
      }
    } catch (e) {
      console.warn('Failed to fetch disputes, loading mock:', e);
    }
  };

  const requestAdditionalDocs = (ptId: string) => {
    console.log('Request additional docs is stubbed locally for PT ID:', ptId);
  };

  const blockPT = async (ptId: string) => {
    try {
      const res = await api.patch(`/admin/users/${ptId}/ban`);
      if (res.data?.success) {
        fetchBackendData();
        refreshUsersList();
      }
    } catch (e) {
      console.error('Failed to block PT user:', e);
    }
  };

  const releaseSettlement = async (settlementId: string) => {
    try {
      const res = await api.post(`/admin/settlements/${settlementId}/approve`);
      if (res.data?.success) {
        fetchBackendData();
      }
    } catch (e) {
      console.error('Failed to release settlement:', e);
    }
  };

  const markNotificationsAsRead = async () => {
    setNotifications(prev => prev.map(n => ({ ...n, read: true })));
    try {
      await api.patch('/notifications/read-all');
    } catch (e) {
      console.error('Failed to mark notifications as read:', e);
    }
  };

  const markNotificationAsRead = async (id: string) => {
    setNotifications(prev => prev.map(n => n.id === id ? { ...n, read: true } : n));
    try {
      await api.patch(`/notifications/${id}/read`);
    } catch (e) {
      console.error('Failed to mark notification as read:', e);
    }
  };

  return (
    <DataContext.Provider
      value={{
        packages,
        bookings,
        workspaces,
        chatMessages,
        mealPlans,
        reviews,
        disputes,
        settlements,
        auditLogs,
        notifications,
        transactions,
        ptVerifications,
        createPackage,
        updatePackage,
        createBooking,
        cancelBooking,
        simulatePayOSPayment,
        acceptBooking,
        rejectBooking,
        completeCourse,
        sendChatMessage,
        saveWorkspaceNotes,
        incrementWorkspaceSession,
        triggerMealPlanGeneration,
        addReview,
        fileDispute,
        resolveDisputeAction,
        approvePTVerification,
        rejectPTVerification,
        requestAdditionalDocs,
        blockPT,
        releaseSettlement,
        markNotificationsAsRead,
        markNotificationAsRead,
        refreshDisputes,
        refreshBackendData: fetchBackendData
      }}
    >
      {children}
    </DataContext.Provider>
  );
};

export const useData = () => {
  const context = useContext(DataContext);
  if (!context) throw new Error('useData must be used within a DataProvider');
  return context;
};
