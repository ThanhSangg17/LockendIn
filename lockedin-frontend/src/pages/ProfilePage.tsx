import React, { useState, useEffect } from 'react';
import { Link } from 'react-router-dom';
import { User, CreditCard, TrendingUp, Camera, Edit3, CheckCircle, AlertCircle, Key, Loader2, Shield, DollarSign, Trash2, Star, Home, Search, Calendar, MessageSquare } from 'lucide-react';
import Badge from '../components/Badge';
import { useAuth } from '../context/AuthContext';
import api from '../services/api';

const TRANSACTIONS = [
  { id: 'TX001', date: '28/05/2026', pt: 'Nguyễn Văn Hùng', sessions: 8, amount: 3600000, status: 'completed' },
  { id: 'TX002', date: '15/05/2026', pt: 'Trần Thị Lan', sessions: 4, amount: 1520000, status: 'completed' },
  { id: 'TX003', date: '02/05/2026', pt: 'Phạm Minh Đức', sessions: 12, amount: 5040000, status: 'active' },
  { id: 'TX004', date: '10/04/2026', pt: 'Lê Quốc Bảo', sessions: 4, amount: 2400000, status: 'refunded' },
  { id: 'TX005', date: '01/03/2026', pt: 'Võ Thị Hương', sessions: 8, amount: 2560000, status: 'pending' },
];

const SCAN_HISTORY = [
  { date: 'Tháng 1/2026', weight: 85.2, bodyFat: 28.5, muscle: 58.4, bmi: 27.8 },
  { date: 'Tháng 2/2026', weight: 82.1, bodyFat: 25.8, muscle: 59.7, bmi: 26.8 },
  { date: 'Tháng 3/2026', weight: 79.5, bodyFat: 23.1, muscle: 61.2, bmi: 26.0 },
  { date: 'Tháng 4/2026', weight: 77.0, bodyFat: 21.4, muscle: 62.8, bmi: 25.2 },
  { date: 'Tháng 5/2026', weight: 74.8, bodyFat: 19.7, muscle: 63.9, bmi: 24.5 },
];

// SVG BMI Gauge
const BMIGauge: React.FC<{ value: number }> = ({ value }) => {
  const min = 15, max = 40;
  const pct = Math.min(Math.max((value - min) / (max - min), 0), 1);
  const angle = -140 + pct * 280;
  const rad = (angle * Math.PI) / 180;
  const cx = 100, cy = 100, r = 70;
  const nx = cx + r * Math.cos(rad);
  const ny = cy + r * Math.sin(rad);

  const getCategory = (v: number) => {
    if (v < 18.5) return { label: 'Thiếu Cân', color: '#999' };
    if (v < 25) return { label: 'Bình Thường', color: '#ffffff' };
    if (v < 30) return { label: 'Thừa Cân', color: '#E60000' };
    return { label: 'Béo Phì', color: '#CC0000' };
  };
  const cat = getCategory(value);

  return (
    <div className="flex flex-col items-center">
      <svg viewBox="0 0 200 140" className="w-48 h-auto">
        {/* Background arc */}
        <path d="M 25 115 A 75 75 0 0 1 175 115" stroke="#1E1E1E" strokeWidth="12" fill="none" strokeLinecap="round" />
        {/* Colored arc */}
        <path d="M 25 115 A 75 75 0 0 1 175 115" stroke="#E60000" strokeWidth="12" fill="none" strokeLinecap="round"
          strokeDasharray={`${pct * 235} 235`} />
        {/* Needle */}
        <line x1={cx} y1={cy} x2={nx} y2={ny} stroke="white" strokeWidth="2" strokeLinecap="round" />
        <circle cx={cx} cy={cy} r="5" fill="white" />
        {/* BMI value */}
        <text x={cx} y={cy + 28} textAnchor="middle" fill="white" fontSize="22" fontWeight="700" fontFamily="Bebas Neue, sans-serif">{value}</text>
        <text x={cx} y={cy + 44} textAnchor="middle" fill="#666" fontSize="8" fontFamily="Inter, sans-serif" letterSpacing="2">BMI</text>
      </svg>
      <span className="text-xs font-semibold uppercase tracking-widest" style={{ color: cat.color }}>{cat.label}</span>
    </div>
  );
};

const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  const map: Record<string, { variant: 'red' | 'white' | 'gray' | 'outline', label: string }> = {
    completed: { variant: 'white', label: 'Hoàn Thành' },
    active: { variant: 'red', label: 'Đang Hoạt Động' },
    refunded: { variant: 'gray', label: 'Hoàn Tiền' },
    pending: { variant: 'outline', label: 'Chờ Xử Lý' },
  };
  const { variant, label } = map[status] || { variant: 'gray', label: status };
  return <Badge variant={variant}>{label}</Badge>;
};

const ProfilePage: React.FC = () => {
  const { currentUser, updateProfile } = useAuth();
  const [activeTab, setActiveTab] = useState('profile');
  const [editing, setEditing] = useState(false);
  const [formFields, setFormFields] = useState({
    fullName: '',
    phoneNumber: '',
    gender: 'male',
    height: 175,
    weight: 74.8,
    bio: '',
    experienceYears: 1,
  });

  const handleStartEdit = () => {
    setFormFields({
      fullName: currentUser?.fullName || '',
      phoneNumber: currentUser?.phoneNumber || '0901 234 567',
      gender: currentUser?.gender || 'male',
      height: currentUser?.height || 175,
      weight: currentUser?.weight || 74.8,
      bio: currentUser?.bio || '',
      experienceYears: currentUser?.experienceYears || 1,
    });
    setEditing(true);
  };

  const handleSaveProfile = async () => {
    try {
      await updateProfile({
        fullName: formFields.fullName,
        phoneNumber: formFields.phoneNumber,
        gender: formFields.gender as any,
        height: Number(formFields.height),
        weight: Number(formFields.weight),
        bio: formFields.bio,
        experienceYears: Number(formFields.experienceYears),
      });
      setEditing(false);
    } catch (e) {
      console.error('Failed to save profile:', e);
    }
  };

  const getTabs = () => {
    if (currentUser?.role === 'pt') {
      return [
        { id: 'profile', label: 'Hồ Sơ HLV', icon: User },
        { id: 'documents', label: 'Minh Chứng', icon: Shield },
        { id: 'transactions', label: 'Doanh Thu', icon: DollarSign },
        { id: 'reviews', label: 'Đánh Giá', icon: Star },
        { id: 'password', label: 'Mật Khẩu', icon: Key },
      ];
    }
    return [
      { id: 'profile', label: 'Hồ Sơ', icon: User },
      { id: 'fitness', label: 'Thể Trạng', icon: TrendingUp },
      { id: 'transactions', label: 'Giao Dịch', icon: CreditCard },
      { id: 'password', label: 'Mật Khẩu', icon: Key },
    ];
  };

  const getQuickStats = () => {
    if (currentUser?.role === 'pt') {
      let statusText = 'Chưa xác minh';
      if (currentUser.verificationStatus === 'verified') statusText = 'Đã xác minh';
      else if (currentUser.verificationStatus === 'pending') statusText = 'Chờ duyệt';
      else if (currentUser.verificationStatus === 'rejected') statusText = 'Bị từ chối';

      return [
        { label: 'Kinh Nghiệm', value: `${currentUser.experienceYears || 1} Năm` },
        { label: 'Tiểu Sư', value: currentUser.bio ? 'Đã thiết lập' : 'Chưa thiết lập' },
        { label: 'Xác Minh', value: statusText },
        { label: 'Thành Viên Từ', value: 'T1/2026' },
      ];
    }
    return [
      { label: 'Buổi Tập', value: '48' },
      { label: 'HLV Đã Thuê', value: '3' },
      { label: 'Mục Tiêu', value: currentUser?.fitnessGoal || 'Giảm Cân' },
      { label: 'Thành Viên Từ', value: 'T1/2026' },
    ];
  };

  const tabsList = getTabs();
  const quickStats = getQuickStats();

  const [documents, setDocuments] = useState<any[]>([]);
  const [loadingDocs, setLoadingDocs] = useState(false);
  const [uploadingDoc, setUploadingDoc] = useState(false);
  const [personalIdFile, setPersonalIdFile] = useState<File | null>(null);
  const [certificateFile, setCertificateFile] = useState<File | null>(null);
  const { requestVerification } = useAuth();

  // PT Settlements state
  const [ptSettlements, setPtSettlements] = useState<any[]>([]);
  const [loadingSettlements, setLoadingSettlements] = useState(false);

  // PT Reviews state
  const [ptReviews, setPtReviews] = useState<any[]>([]);
  const [loadingReviews, setLoadingReviews] = useState(false);
  const [replyText, setReplyText] = useState<Record<string, string>>({});
  const [submittingReply, setSubmittingReply] = useState<Record<string, boolean>>({});

  const loadPtReviews = async () => {
    try {
      setLoadingReviews(true);
      const res = await api.get('/reviews/my');
      if (res.data?.success) {
        setPtReviews(res.data.data || []);
      }
    } catch (e) {
      console.error('Failed to load reviews:', e);
    } finally {
      setLoadingReviews(false);
    }
  };

  const loadPtDocuments = async () => {
    try {
      setLoadingDocs(true);
      const res = await api.get('/pts/me/documents');
      if (res.data?.success) {
        setDocuments(res.data.data || []);
      }
    } catch (e) {
      console.error('Failed to load documents:', e);
    } finally {
      setLoadingDocs(false);
    }
  };

  const loadPtSettlements = async () => {
    try {
      setLoadingSettlements(true);
      const res = await api.get('/settlements/my');
      let data = res.data?.data || [];
      const mockData = [
        { id: 'ST001', bookingId: 'BK1234', netAmount: 1800000, createdAt: '2026-05-15T10:00:00Z', status: 2 },
        { id: 'ST002', bookingId: 'BK1235', netAmount: 2400000, createdAt: '2026-06-01T14:30:00Z', status: 2 },
        { id: 'ST003', bookingId: 'BK1236', netAmount: 3200000, createdAt: '2026-06-20T09:15:00Z', status: 1 },
        { id: 'ST004', bookingId: 'BK1237', netAmount: 1500000, createdAt: '2026-06-22T16:45:00Z', status: 0 },
      ];
      data = [...data, ...mockData];
      if (res.data?.success || data.length > 0) {
        setPtSettlements(data);
      }
    } catch (e) {
      console.error('Failed to load settlements:', e);
    } finally {
      setLoadingSettlements(false);
    }
  };

  useEffect(() => {
    if (currentUser?.role === 'pt') {
      if (activeTab === 'documents') {
        loadPtDocuments();
      } else if (activeTab === 'transactions') {
        loadPtSettlements();
      } else if (activeTab === 'reviews') {
        loadPtReviews();
      }
    }
  }, [activeTab, currentUser]);

  // Change Password state
  const [currentPassword, setCurrentPassword] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [pwLoading, setPwLoading] = useState(false);
  const [pwSuccess, setPwSuccess] = useState('');
  const [pwError, setPwError] = useState('');

  const handleChangePassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setPwSuccess('');
    setPwError('');

    if (newPassword !== confirmPassword) {
      setPwError('Mật khẩu mới và mật khẩu xác nhận không khớp.');
      return;
    }

    const passRegex = /^(?=.*[a-zA-Z])(?=.*\d).+$/;
    if (newPassword.length < 8 || !passRegex.test(newPassword)) {
      setPwError('Mật khẩu mới phải dài tối thiểu 8 ký tự và chứa ít nhất một chữ cái và một chữ số.');
      return;
    }

    setPwLoading(true);
    try {
      const res = await api.put('/users/me/password', {
        currentPassword,
        newPassword
      });
      if (res.data?.success) {
        setPwSuccess(res.data?.message || 'Đổi mật khẩu thành công!');
        setCurrentPassword('');
        setNewPassword('');
        setConfirmPassword('');
      } else {
        setPwError(res.data?.message || 'Đổi mật khẩu thất bại.');
      }
    } catch (err: any) {
      setPwError(err.response?.data?.message || err.message || 'Lỗi kết nối máy chủ.');
    } finally {
      setPwLoading(false);
    }
  };

  const latestScan = SCAN_HISTORY[SCAN_HISTORY.length - 1];
  const firstScan = SCAN_HISTORY[0];

  if (currentUser?.role === 'pt') {
    return (
      <div className="min-h-screen bg-brand-black mobile-content-pad">
        {/* Cover Photo/Banner */}
        <div className="relative h-48 md:h-64 overflow-hidden bg-brand-black">
          {/* Background Image */}
          <div 
            className="absolute inset-0 bg-cover bg-center bg-no-repeat opacity-40 grayscale-[30%]" 
            style={{ backgroundImage: "url('https://images.unsplash.com/photo-1534438327276-14e5300c3a48?q=80&w=1470&auto=format&fit=crop')" }} 
          />
          {/* Dark Red Gradient Overlays */}
          <div className="absolute inset-0 bg-gradient-to-r from-brand-black via-brand-black/80 to-brand-red/30" />
          <div className="absolute inset-0 bg-gradient-to-t from-brand-black via-brand-black/20 to-transparent" />
          
          <div className="absolute inset-0 bg-[radial-gradient(circle_at_top_right,_var(--tw-gradient-stops))] from-brand-red/20 via-transparent to-transparent pointer-events-none" />
          
          {/* Subtle Grid overlay for depth */}
          <div className="absolute inset-0 opacity-[0.05] pointer-events-none" style={{ backgroundImage: 'linear-gradient(to right, #ffffff 1px, transparent 1px), linear-gradient(to bottom, #ffffff 1px, transparent 1px)', backgroundSize: '24px 24px' }}></div>
        </div>

        {/* Profile Info Header Section (Overlapping Cover) */}
        <div className="max-w-6xl mx-auto px-4 sm:px-8 lg:px-12 w-full relative z-10 -mt-16 md:-mt-20 mb-8">
          <div 
            className="flex flex-col md:flex-row items-center gap-6 w-full text-center md:text-left p-6 md:p-8 rounded-2xl shadow-2xl backdrop-blur-[12px] bg-white/[0.05] border border-white/10"
          >
            {/* Profile Avatar */}
            <div className="relative w-32 h-32 md:w-40 md:h-40 flex-shrink-0 z-10">
              <div 
                className="w-full h-full bg-brand-black border-4 border-brand-black rounded-full flex items-center justify-center overflow-hidden aspect-square transition-transform hover:scale-105 duration-300"
                style={{ boxShadow: '0 0 15px #ef4444, 0 0 30px #ef4444' }}
              >
                {currentUser?.avatar ? (
                  <img src={currentUser.avatar} alt="Avatar" className="w-full h-full object-cover" />
                ) : (
                  <span className="font-display font-semibold text-3xl md:text-4xl text-brand-red">
                    {currentUser?.fullName ? currentUser.fullName.split(' ').map((n: string) => n[0]).join('').substring(0, 2).toUpperCase() : 'PT'}
                  </span>
                )}
              </div>
              <input
                type="file"
                id="avatar-upload-pt"
                className="hidden"
                accept="image/*"
                onChange={async (e) => {
                  const file = e.target.files?.[0];
                  if (!file) return;
                  const formData = new FormData();
                  formData.append('file', file);
                  
                  const handleUpdateAvatar = async (url: string) => {
                    const updateRes = await api.put('/users/me/avatar', { avatarUrl: url });
                    if (updateRes.data?.success) {
                      alert('Cập nhật ảnh đại diện thành công!');
                      window.location.reload();
                    } else {
                      alert('Cập nhật ảnh đại diện thất bại.');
                    }
                  };

                  try {
                    const uploadRes = await api.post('/uploads/image?folder=avatars', formData, {
                      headers: {
                        'Content-Type': 'multipart/form-data',
                      },
                    });
                    if (uploadRes.data?.success) {
                      const fileUrl = uploadRes.data.data.secureUrl || uploadRes.data.data.url;
                      await handleUpdateAvatar(fileUrl);
                    } else {
                      throw new Error('Upload failed');
                    }
                  } catch (err) {
                    console.warn("Upload API failed, falling back to Base64:", err);
                    const reader = new FileReader();
                    reader.readAsDataURL(file);
                    reader.onloadend = async () => {
                      try {
                        await handleUpdateAvatar(reader.result as string);
                      } catch (updateErr) {
                        console.error(updateErr);
                        alert('Cập nhật ảnh đại diện thất bại.');
                      }
                    };
                  }
                }}
              />
              <button
                onClick={() => document.getElementById('avatar-upload-pt')?.click()}
                className="absolute bottom-1 right-1 md:bottom-2 md:right-2 w-10 h-10 rounded-full bg-brand-red hover:bg-brand-red-dark flex items-center justify-center cursor-pointer transition-colors border-2 border-brand-black shadow-[0_0_10px_#ef4444]"
              >
                <Camera size={16} className="text-white" />
              </button>
            </div>

            {/* HLV Name and Info */}
            <div className="flex-1 pt-4 md:pt-0">
              <div className="flex flex-col md:flex-row items-center gap-3 justify-center md:justify-start mb-1">
                <h1 className="font-montserrat font-extrabold text-2xl md:text-3xl text-white uppercase tracking-wider">
                  {currentUser?.fullName || 'Huấn Luyện Viên'}
                </h1>
                <span className="bg-brand-red/10 border border-brand-red/30 text-brand-red text-[10px] font-bold px-2 py-0.5 tracking-widest uppercase">
                  PRO COACH
                </span>
              </div>
              <p className="text-white/60 text-xs md:text-sm font-medium">{currentUser?.email || 'tài khoản HLV'}</p>
            </div>

            {/* Quick Status Badge */}
            <div className="flex flex-col items-center md:items-end gap-2 mt-4 md:mt-0">
              <span className="text-[10px] text-white/50 tracking-widest uppercase font-bold">Trạng Thái Hồ Sơ</span>
              <span className={`px-4 py-2 rounded-xl text-xs font-bold uppercase tracking-widest border ${
                currentUser.verificationStatus === 'verified'
                  ? 'border-brand-red bg-brand-red/20 text-brand-red shadow-[0_0_15px_rgba(239,68,68,0.2)]'
                  : currentUser.verificationStatus === 'pending'
                  ? 'border-white/20 bg-white/10 text-white/90'
                  : 'border-brand-red bg-brand-red/20 text-brand-red'
              }`}>
                {currentUser.verificationStatus === 'verified' ? 'Đã Xác Minh' : currentUser.verificationStatus === 'pending' ? 'Chờ Duyệt' : 'Bị Từ Chối'}
              </span>
            </div>
          </div>
        </div>

        {/* Tab Navigation Menu (Integrated Below Banner) */}
        <div className="bg-brand-surface border-b border-brand-border sticky top-0 z-20">
          <div className="max-w-6xl mx-auto px-4 sm:px-8 lg:px-12 w-full flex overflow-x-auto scrollbar-none gap-2">
            {tabsList.map((tab) => {
              const Icon = tab.icon;
              return (
                <button
                  key={tab.id}
                  onClick={() => setActiveTab(tab.id)}
                  className={`flex items-center gap-2 px-6 py-5 text-xs font-bold uppercase tracking-widest whitespace-nowrap transition-all duration-200 cursor-pointer border-b-2 ${
                    activeTab === tab.id
                      ? 'text-brand-red border-brand-red bg-brand-red/[0.02]'
                      : 'text-white/40 border-transparent hover:text-white hover:border-white/10'
                  }`}
                >
                  <Icon size={14} className={activeTab === tab.id ? 'text-brand-red' : 'text-white/40'} />
                  {tab.label}
                </button>
              );
            })}
          </div>
        </div>

        {/* Main Content Area */}
        <div className="max-w-6xl mx-auto px-4 sm:px-8 lg:px-12 w-full py-8 md:py-12">
          <div className="grid lg:grid-cols-4 gap-8">
            {/* Left Column: Side info stats */}
            <div className="lg:col-span-1 flex flex-col gap-6">
              {/* Profile Card Summary */}
              <div className="backdrop-blur-[12px] bg-white/[0.05] border border-white/10 shadow-xl rounded-2xl p-6 relative overflow-hidden">
                <div className="absolute top-0 right-0 w-24 h-24 bg-brand-red/20 rounded-full blur-3xl -mr-8 -mt-8 pointer-events-none" />
                <h3 className="font-montserrat font-bold text-xs uppercase tracking-wider text-brand-red mb-4 border-b border-brand-border pb-2">
                  TỔNG QUAN NĂNG LỰC
                </h3>
                <div className="flex flex-col gap-4">
                  {quickStats.map((stat, i) => (
                    <div key={i} className="flex flex-col">
                      <span className="text-[10px] text-white/30 uppercase tracking-widest font-semibold">{stat.label}</span>
                      <span className="text-white font-bold text-sm mt-0.5">{stat.value}</span>
                    </div>
                  ))}
                </div>
              </div>

              {/* Status Notice */}
              <div className="backdrop-blur-[12px] bg-white/[0.05] border border-white/10 shadow-xl rounded-2xl p-6 relative overflow-hidden">
                <h4 className="font-montserrat font-bold text-xs uppercase tracking-wider text-white/70 mb-3">XÁC MINH DANH TÍNH</h4>
                <p className="text-xs text-white/40 leading-relaxed">
                  Để tăng khả năng nhận lớp và tối ưu hóa doanh thu, hãy chắc chắn rằng bạn đã cập nhật đầy đủ CCCD và các chứng chỉ thể hình cần thiết ở phần **Minh Chứng**.
                </p>
              </div>
            </div>

            {/* Right Column: Tab Content */}
            <div className="lg:col-span-3 min-w-0 w-full backdrop-blur-[12px] bg-white/[0.05] border border-white/10 shadow-2xl rounded-2xl p-6 md:p-8 relative overflow-hidden">
              {/* Tab Content Rendering */}
              {activeTab === 'profile' && (
                <div className="animate-fade-in">
                  <div className="flex items-center justify-between mb-6">
                    <h3 className="font-montserrat font-extrabold text-xl text-white uppercase tracking-wider">Thông Tin Cá Nhân HLV</h3>
                    <button
                      onClick={() => { if (!editing) handleStartEdit(); else setEditing(false); }}
                      className={`flex items-center gap-2 text-xs uppercase tracking-widest cursor-pointer transition-colors duration-200 ${editing ? 'text-brand-red' : 'text-white/40 hover:text-white'}`}
                    >
                      <Edit3 size={14} />
                      {editing ? 'Đang Chỉnh Sửa...' : 'Chỉnh Sửa'}
                    </button>
                  </div>

                  <div className="grid md:grid-cols-2 gap-4">
                    {editing ? (
                      <>
                        <div>
                          <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Họ và Tên</label>
                          <input
                            type="text"
                            value={formFields.fullName}
                            onChange={(e) => setFormFields({ ...formFields, fullName: e.target.value })}
                            className="input-dark focus:border-brand-red"
                          />
                        </div>
                        <div>
                          <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Email (Không thể thay đổi)</label>
                          <p className="text-white/40 text-sm px-4 py-3 bg-brand-surface border border-brand-border">{currentUser?.email}</p>
                        </div>
                        <div>
                          <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Số Điện Thoại</label>
                          <input
                            type="text"
                            value={formFields.phoneNumber}
                            onChange={(e) => setFormFields({ ...formFields, phoneNumber: e.target.value })}
                            className="input-dark focus:border-brand-red"
                          />
                        </div>
                        <div>
                          <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Số Năm Kinh Nghiệm</label>
                          <input
                            type="number"
                            value={formFields.experienceYears}
                            onChange={(e) => setFormFields({ ...formFields, experienceYears: Number(e.target.value) })}
                            className="input-dark focus:border-brand-red"
                          />
                        </div>
                        <div className="md:col-span-2">
                          <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Tiểu Sử HLV</label>
                          <textarea
                            value={formFields.bio}
                            onChange={(e) => setFormFields({ ...formFields, bio: e.target.value })}
                            className="input-dark w-full h-32 resize-none focus:border-brand-red"
                          />
                        </div>
                      </>
                    ) : (
                      <>
                        <div>
                          <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Họ và Tên</label>
                          <p className="text-white text-sm px-4 py-3 bg-brand-dark border border-brand-border">{currentUser?.fullName || ''}</p>
                        </div>
                        <div>
                          <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Email</label>
                          <p className="text-white text-sm px-4 py-3 bg-brand-dark border border-brand-border">{currentUser?.email || ''}</p>
                        </div>
                        <div>
                          <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Số Điện Thoại</label>
                          <p className="text-white text-sm px-4 py-3 bg-brand-dark border border-brand-border">{currentUser?.phoneNumber || 'Chưa thiết lập'}</p>
                        </div>
                        <div>
                          <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Số Năm Kinh Nghiệm</label>
                          <p className="text-white text-sm px-4 py-3 bg-brand-dark border border-brand-border">{currentUser?.experienceYears || '0'} Năm</p>
                        </div>
                        <div className="md:col-span-2">
                          <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Tiểu Sử HLV</label>
                          <p className="text-white text-sm px-4 py-3 bg-brand-dark border border-brand-border whitespace-pre-line leading-relaxed">{currentUser?.bio || 'Chưa thiết lập tiểu sử.'}</p>
                        </div>
                      </>
                    )}
                  </div>

                  {editing && (
                    <div className="flex gap-3 mt-6">
                      <button onClick={handleSaveProfile} className="btn-primary text-xs py-3 px-8 bg-brand-red border-brand-red hover:bg-brand-red-dark text-white font-bold">Lưu Thay Đổi</button>
                      <button onClick={() => setEditing(false)} className="btn-secondary text-xs py-3 px-8">Hủy</button>
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'documents' && (
                <div className="animate-fade-in">
                  <h3 className="font-montserrat font-extrabold text-xl text-white uppercase tracking-wider mb-2">Hồ Sơ Minh Chứng HLV</h3>
                  <p className="text-white/40 text-xs uppercase tracking-widest mb-8">Tải CCCD và chứng chỉ để được duyệt hiển thị trên Marketplace</p>

                  <div className="bg-brand-dark border border-brand-border p-6 mb-8">
                    <h4 className="font-semibold text-white mb-4 uppercase text-xs tracking-wider text-brand-red">Tải lên hồ sơ minh chứng mới</h4>
                    <div className="grid md:grid-cols-2 gap-4 mb-4">
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Ảnh CCCD / Hộ Chiếu</label>
                        {personalIdFile ? (
                          <div className="relative border border-brand-border bg-brand-surface p-2 flex items-center justify-between h-12">
                            <span className="text-white/60 text-xs truncate max-w-[80%]">{personalIdFile.name}</span>
                            <button
                              type="button"
                              onClick={() => setPersonalIdFile(null)}
                              className="text-brand-red hover:text-white text-xs uppercase font-bold tracking-wider cursor-pointer"
                            >
                              Xóa
                            </button>
                          </div>
                        ) : (
                          <div className="relative">
                            <input
                              type="file"
                              accept="image/*"
                              disabled={uploadingDoc}
                              onChange={(e) => {
                                const file = e.target.files?.[0];
                                if (file) setPersonalIdFile(file);
                              }}
                              className="hidden"
                              id="personal-id-file-input"
                            />
                            <button
                              type="button"
                              onClick={() => document.getElementById('personal-id-file-input')?.click()}
                              disabled={uploadingDoc}
                              className="w-full flex items-center justify-center gap-2 border border-dashed border-white/20 hover:border-brand-red bg-brand-surface hover:bg-brand-surface/50 text-white/60 hover:text-white py-3 px-4 text-xs font-semibold uppercase tracking-wider transition-colors duration-200 cursor-pointer h-12"
                            >
                              Chọn file ảnh CCCD
                            </button>
                          </div>
                        )}
                      </div>

                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Chứng chỉ nghề HLV / Bằng cấp liên quan</label>
                        {certificateFile ? (
                          <div className="relative border border-brand-border bg-brand-surface p-2 flex items-center justify-between h-12">
                            <span className="text-white/60 text-xs truncate max-w-[80%]">{certificateFile.name}</span>
                            <button
                              type="button"
                              onClick={() => setCertificateFile(null)}
                              className="text-brand-red hover:text-white text-xs uppercase font-bold tracking-wider cursor-pointer"
                            >
                              Xóa
                            </button>
                          </div>
                        ) : (
                          <div className="relative">
                            <input
                              type="file"
                              accept="image/*"
                              disabled={uploadingDoc}
                              onChange={(e) => {
                                const file = e.target.files?.[0];
                                if (file) setCertificateFile(file);
                              }}
                              className="hidden"
                              id="certificate-file-input"
                            />
                            <button
                              type="button"
                              onClick={() => document.getElementById('certificate-file-input')?.click()}
                              disabled={uploadingDoc}
                              className="w-full flex items-center justify-center gap-2 border border-dashed border-white/20 hover:border-brand-red bg-brand-surface hover:bg-brand-surface/50 text-white/60 hover:text-white py-3 px-4 text-xs font-semibold uppercase tracking-wider transition-colors duration-200 cursor-pointer h-12"
                            >
                              Chọn file chứng chỉ
                            </button>
                          </div>
                        )}
                      </div>
                    </div>
                    <button
                      onClick={async () => {
                        if (!personalIdFile) {
                          alert('Vui lòng tải lên ảnh CCCD để xác minh danh tính!');
                          return;
                        }
                        setUploadingDoc(true);
                        try {
                          await requestVerification(
                            personalIdFile,
                            certificateFile ? [certificateFile] : []
                          );
                          
                          alert('Gửi hồ sơ minh chứng thành công! Đang chờ Admin xét duyệt.');
                          setPersonalIdFile(null);
                          setCertificateFile(null);
                          loadPtDocuments();
                        } catch (err) {
                          console.error('Failed to upload docs:', err);
                          alert('Gửi hồ sơ minh chứng thất bại.');
                        } finally {
                          setUploadingDoc(false);
                        }
                      }}
                      disabled={uploadingDoc}
                      className="btn-primary py-2.5 px-6 text-xs font-bold uppercase tracking-wider cursor-pointer bg-brand-red border-brand-red hover:bg-brand-red-dark text-white"
                    >
                      {uploadingDoc ? 'Đang gửi...' : 'Gửi Hồ Sơ Xét Duyệt'}
                    </button>
                  </div>

                  <h4 className="font-semibold text-white mb-4 uppercase text-xs tracking-wider">Danh sách minh chứng hiện tại</h4>
                  {loadingDocs ? (
                    <div className="text-center py-8 text-white/40">
                      <Loader2 className="animate-spin mx-auto mb-2 text-brand-red" size={24} />
                      Đang tải danh sách tài liệu...
                    </div>
                  ) : documents.length === 0 ? (
                    <div className="bg-brand-dark border border-brand-border p-12 text-center text-white/30 uppercase tracking-widest text-[10px]">
                      Chưa tải lên minh chứng nào
                    </div>
                  ) : (
                    <div className="grid md:grid-cols-2 gap-6">
                      {documents.map((doc) => (
                        <div key={doc.id} className="bg-brand-dark border border-brand-border p-4 flex flex-col justify-between">
                          <div>
                            <div className="flex items-center justify-between mb-3">
                              <span className="text-[10px] uppercase font-bold tracking-widest text-white/40">
                                {doc.documentType === 1 ? 'CCCD / Hộ Chiếu' : 'Chứng Chỉ Chuyên Môn'}
                              </span>
                              <Badge variant={doc.status === 2 ? 'white' : doc.status === 1 ? 'red' : 'gray'}>
                                {doc.status === 2 ? 'Đã Duyệt' : doc.status === 1 ? 'Chờ Duyệt' : 'Bị Từ Chối'}
                              </Badge>
                            </div>
                            <div className="h-40 bg-brand-surface border border-brand-border mb-4 overflow-hidden flex items-center justify-center">
                              <img src={doc.fileUrl} alt="Minh chứng" className="max-h-full max-w-full object-contain" />
                            </div>
                          </div>
                          <button
                            onClick={async () => {
                              if (!window.confirm('Bạn có chắc chắn muốn xóa tài liệu này không?')) return;
                              try {
                                await api.delete(`/pts/me/documents/${doc.id}`);
                                loadPtDocuments();
                              } catch (err) {
                                console.error(err);
                                alert('Xóa tài liệu thất bại.');
                              }
                            }}
                            className="w-full py-2 bg-brand-surface hover:bg-brand-red/10 border border-brand-border hover:border-brand-red text-white/50 hover:text-brand-red text-xs uppercase tracking-widest transition-all cursor-pointer flex items-center justify-center gap-1.5 font-bold"
                          >
                            <Trash2 size={12} /> XÓA TÀI LIỆU
                          </button>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'transactions' && (
                <div className="animate-fade-in">
                  <h3 className="font-montserrat font-extrabold text-xl text-white uppercase tracking-wider mb-8">Lịch Sử Giải Ngân</h3>
                  
                  <div className="grid grid-cols-3 gap-4 mb-8">
                    {[
                      { label: 'Doanh Thu Lũy Kế', value: `₫${ptSettlements.reduce((acc, curr) => curr.status === 2 ? acc + curr.netAmount : acc, 0).toLocaleString('vi-VN')}` },
                      { label: 'Tổng Yêu Cầu', value: ptSettlements.length.toString() },
                      { label: 'Chờ Giải Ngân', value: `₫${ptSettlements.reduce((acc, curr) => curr.status === 1 ? acc + curr.netAmount : acc, 0).toLocaleString('vi-VN')}` },
                    ].map((s, i) => (
                      <div key={i} className="bg-brand-dark border border-brand-border p-5 text-center">
                        <p className="text-white/30 text-[10px] uppercase tracking-widest mb-2 font-semibold">{s.label}</p>
                        <p className="font-mono font-bold text-lg md:text-xl text-brand-red truncate">{s.value}</p>
                      </div>
                    ))}
                  </div>

                  {loadingSettlements ? (
                    <div className="text-center py-8 text-white/40">
                      <Loader2 className="animate-spin mx-auto mb-2 text-brand-red" size={24} />
                      Đang tải thông tin giải ngân...
                    </div>
                  ) : ptSettlements.length === 0 ? (
                    <div className="bg-brand-dark border border-brand-border p-12 text-center text-white/30 uppercase tracking-widest text-[10px]">
                      Chưa có lịch sử giải ngân nào
                    </div>
                  ) : (
                    <div className="bg-brand-dark border border-brand-border overflow-x-auto w-full max-w-full">
                      <div className="grid grid-cols-5 px-6 py-3 border-b border-brand-border min-w-[600px] bg-brand-surface">
                        {['Mã Giải Ngân', 'Mã Đơn Tập', 'Số Tiền', 'Ngày Tạo', 'Trạng Thái'].map((h) => (
                          <span key={h} className="text-white/30 text-[10px] uppercase tracking-widest font-bold">{h}</span>
                        ))}
                      </div>
                      {ptSettlements.map((settle) => (
                        <div key={settle.id} className="grid grid-cols-5 px-6 py-4 border-b border-brand-border last:border-0 hover:bg-white/[0.02] transition-colors duration-150 min-w-[600px]">
                          <span className="text-white/50 text-xs font-mono truncate pr-2">{settle.id}</span>
                          <span className="text-white/50 text-xs font-mono truncate pr-2">{settle.bookingId}</span>
                          <span className="text-white text-xs font-semibold">{settle.netAmount.toLocaleString('vi-VN')}đ</span>
                          <span className="text-white/50 text-xs">{new Date(settle.createdAt).toLocaleDateString('vi-VN')}</span>
                          <span>
                            <Badge variant={settle.status === 2 ? 'white' : settle.status === 1 ? 'red' : 'gray'}>
                              {settle.status === 2 ? 'Đã Giải Ngân' : settle.status === 1 ? 'Chờ Duyệt' : 'Bị Khóa'}
                            </Badge>
                          </span>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'reviews' && (
                <div className="animate-fade-in">
                  <h3 className="font-montserrat font-extrabold text-xl text-white uppercase tracking-wider mb-2">Đánh Giá Từ Học Viên</h3>
                  <p className="text-white/40 text-xs uppercase tracking-widest mb-8">Danh sách nhận xét và gửi phản hồi cho học viên của bạn</p>

                  {loadingReviews ? (
                    <div className="text-center py-8 text-white/40">
                      <Loader2 className="animate-spin mx-auto mb-2 text-brand-red" size={24} />
                      Đang tải danh sách đánh giá...
                    </div>
                  ) : ptReviews.length === 0 ? (
                    <div className="bg-brand-dark border border-brand-border p-12 text-center text-white/30 uppercase tracking-widest text-[10px]">
                      Chưa có học viên nào đánh giá bạn
                    </div>
                  ) : (
                    <div className="flex flex-col gap-6">
                      {ptReviews.map((rev: any) => (
                        <div key={rev.id} className="bg-brand-dark border border-brand-border p-6">
                          <div className="flex justify-between items-start mb-3">
                            <div>
                              <span className="text-sm text-white font-semibold">Học viên ẩn danh</span>
                              <p className="text-[10px] text-white/30 font-mono mt-0.5">Mã đánh giá: {rev.id}</p>
                            </div>
                            <span className="text-white/30 text-xs">{new Date(rev.createdAt).toLocaleDateString('vi-VN')}</span>
                          </div>
                          
                          <div className="flex items-center gap-1 mb-3">
                            {Array.from({ length: 5 }).map((_, idx) => (
                              <Star
                                key={idx}
                                size={12}
                                className={idx < rev.rating ? 'text-brand-red fill-brand-red' : 'text-white/20 fill-transparent'}
                              />
                            ))}
                          </div>

                          <p className="text-white/80 text-sm leading-relaxed mb-4">{rev.comment || 'Học viên không viết nhận xét.'}</p>

                          {rev.ptReply ? (
                            <div className="bg-brand-surface border-l-2 border-brand-red p-4 text-xs">
                              <p className="text-brand-red font-semibold mb-1">Phản hồi của bạn:</p>
                              <p className="text-white/70">{rev.ptReply}</p>
                            </div>
                          ) : (
                            <div className="mt-4 pt-4 border-t border-brand-border/40">
                              <div className="flex gap-3">
                                <input
                                  type="text"
                                  value={replyText[rev.id] || ''}
                                  onChange={(e) => setReplyText(prev => ({ ...prev, [rev.id]: e.target.value }))}
                                  placeholder="Viết phản hồi cho đánh giá này..."
                                  className="input-dark flex-1 text-xs py-2 px-3 h-auto focus:border-brand-red"
                                />
                                <button
                                  onClick={async () => {
                                    const text = replyText[rev.id];
                                    if (!text || !text.trim()) {
                                      alert('Vui lòng nhập nội dung phản hồi!');
                                      return;
                                    }
                                    setSubmittingReply(prev => ({ ...prev, [rev.id]: true }));
                                    try {
                                      const res = await api.post(`/reviews/${rev.id}/reply`, {
                                        reply: text.trim()
                                      });
                                      if (res.data?.success) {
                                        alert('Đã gửi phản hồi thành công!');
                                        loadPtReviews();
                                      } else {
                                        alert('Gửi phản hồi thất bại.');
                                      }
                                    } catch (err) {
                                      console.error(err);
                                      alert('Gửi phản hồi thất bại.');
                                    } finally {
                                      setSubmittingReply(prev => ({ ...prev, [rev.id]: false }));
                                    }
                                  }}
                                  disabled={submittingReply[rev.id]}
                                  className="btn-primary py-2 px-4 text-xs font-semibold uppercase tracking-wider cursor-pointer bg-brand-red border-brand-red hover:bg-brand-red-dark text-white"
                                >
                                  {submittingReply[rev.id] ? 'Gửi...' : 'Phản Hồi'}
                                </button>
                              </div>
                            </div>
                          )}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              )}

              {activeTab === 'password' && (
                <div className="animate-fade-in">
                  <h3 className="font-montserrat font-extrabold text-xl text-white uppercase tracking-wider mb-6">Đổi Mật Khẩu HLV</h3>
                  
                  {pwSuccess && (
                    <div className="mb-6 border border-brand-red bg-brand-red/5 px-4 py-3 text-brand-red text-sm">
                      {pwSuccess}
                    </div>
                  )}

                  {pwError && (
                    <div className="mb-6 border border-brand-red bg-brand-red/10 px-4 py-3 text-brand-red text-sm">
                      {pwError}
                    </div>
                  )}

                  <form onSubmit={handleChangePassword} className="max-w-md flex flex-col gap-5">
                    <div>
                      <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Mật khẩu hiện tại</label>
                      <input
                        type="password"
                        value={currentPassword}
                        onChange={(e) => setCurrentPassword(e.target.value)}
                        placeholder="••••••••"
                        required
                        className="input-dark focus:border-brand-red"
                        disabled={pwLoading}
                      />
                    </div>

                    <div>
                      <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Mật khẩu mới</label>
                      <input
                        type="password"
                        value={newPassword}
                        onChange={(e) => setNewPassword(e.target.value)}
                        placeholder="Tối thiểu 8 ký tự (chứa chữ và số)"
                        required
                        className="input-dark focus:border-brand-red"
                        disabled={pwLoading}
                      />
                    </div>

                    <div>
                      <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Xác nhận mật khẩu mới</label>
                      <input
                        type="password"
                        value={confirmPassword}
                        onChange={(e) => setConfirmPassword(e.target.value)}
                        placeholder="Nhập lại mật khẩu mới"
                        required
                        className="input-dark focus:border-brand-red"
                        disabled={pwLoading}
                      />
                    </div>

                    <button
                      type="submit"
                      disabled={pwLoading}
                      className="btn-primary py-3 px-8 text-xs font-bold uppercase tracking-wider justify-center bg-brand-red border-brand-red hover:bg-brand-red-dark text-white"
                    >
                      {pwLoading ? (
                        <div className="flex items-center gap-1.5">
                          <Loader2 className="animate-spin text-white" size={14} />
                          ĐANG CẬP NHẬT...
                        </div>
                      ) : (
                        'CẬP NHẬT MẬT KHẨU'
                      )}
                    </button>
                  </form>
                </div>
              )}
            </div>
          </div>
        </div>

        {/* Mobile Bottom Nav */}
        <nav className="mobile-bottom-bar">
          {[
            { label: 'Trang Chủ', icon: <Home size={20} />, path: '/' },
            { label: 'Tìm PT', icon: <Search size={20} />, path: '/marketplace' },
            { label: 'Lịch', icon: <Calendar size={20} />, path: '/customer/bookings' },
            { label: 'Chat', icon: <MessageSquare size={20} />, path: '/customer/workspace' },
            { label: 'Hồ Sơ', icon: <User size={20} />, path: '/customer/profile' },
          ].map((tab, i) => (
            <Link key={i} to={tab.path} className="flex flex-col items-center gap-1 px-3 py-1 text-white/50 hover:text-brand-red transition-colors">
              <span className="flex items-center justify-center">{tab.icon}</span>
              <span className="text-[10px] uppercase tracking-widest mt-1">{tab.label}</span>
            </Link>
          ))}
        </nav>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-brand-black mobile-content-pad">
      {/* Page Header */}
      <div className="border-b border-brand-border bg-brand-dark">
        <div className="max-w-6xl mx-auto px-4 sm:px-8 lg:px-12 w-full py-10">
          <p className="section-label mb-2">Tài Khoản</p>
          <h1 className="page-title">Hồ Sơ Của Tôi</h1>
        </div>
      </div>

      <div className="max-w-6xl mx-auto px-4 sm:px-8 lg:px-12 w-full py-8">
        <div className="grid lg:grid-cols-3 gap-6 auto-rows-auto">
          
          {/* Cột 1 (span 2 hàng): Identity Card */}
          <div className="lg:row-span-2 bg-[#161616] border border-[#2a2a2a] rounded-xl flex flex-col overflow-hidden">
             <div className="h-20 bg-gradient-to-b from-brand-red/40 to-[#161616]" />
             <div className="px-6 pb-6 flex flex-col items-center flex-1">
                <div className="relative -mt-12 mb-4">
                  <div className="w-24 h-24 bg-brand-dark rounded-full border-2 border-brand-red flex items-center justify-center overflow-hidden shadow-[0_0_15px_rgba(230,51,51,0.5)]">
                      {currentUser?.avatar ? (
                        <img src={currentUser.avatar} alt="Avatar" className="w-full h-full object-cover" />
                      ) : (
                        <span className="font-display font-semibold text-3xl text-white">
                          {currentUser?.fullName ? currentUser.fullName.split(' ').map((n: string) => n[0]).join('').substring(0, 2).toUpperCase() : 'U'}
                        </span>
                      )}
                    <input
                      type="file"
                      id="avatar-upload-customer"
                      className="hidden"
                      accept="image/*"
                      onChange={async (e) => {
                        const file = e.target.files?.[0];
                        if (!file) return;
                        const formData = new FormData();
                        formData.append('file', file);
                        const handleUpdateAvatar = async (url: string) => {
                          const updateRes = await api.put('/users/me/avatar', { avatarUrl: url });
                          if (updateRes.data?.success) {
                            alert('Cập nhật ảnh đại diện thành công!');
                            window.location.reload();
                          } else alert('Cập nhật ảnh đại diện thất bại.');
                        };
                        try {
                          const uploadRes = await api.post('/uploads/image?folder=avatars', formData, {
                            headers: { 'Content-Type': 'multipart/form-data' },
                          });
                          if (uploadRes.data?.success) {
                            await handleUpdateAvatar(uploadRes.data.data.secureUrl || uploadRes.data.data.url);
                          } else throw new Error('Upload failed');
                        } catch (err) {
                          const reader = new FileReader();
                          reader.readAsDataURL(file);
                          reader.onloadend = async () => {
                            try { await handleUpdateAvatar(reader.result as string); }
                            catch (updateErr) { alert('Cập nhật ảnh đại diện thất bại.'); }
                          };
                        }
                      }}
                    />
                  </div>
                  <button
                    onClick={() => document.getElementById('avatar-upload-customer')?.click()}
                    className="absolute bottom-0 right-0 w-8 h-8 bg-brand-red flex items-center justify-center cursor-pointer hover:bg-brand-red-dark transition-colors rounded-full border-2 border-[#161616] z-10"
                  >
                    <Camera size={14} className="text-white" />
                  </button>
                </div>

                <h2 className="font-semibold text-white text-lg text-center leading-tight">{currentUser?.fullName || 'Người Dùng'}</h2>
                <p className="text-white/40 text-xs mb-4 text-center">{currentUser?.email || 'tài khoản'}</p>
                <span className="bg-brand-red/20 text-brand-red text-[10px] px-3 py-1.5 rounded-full font-bold uppercase tracking-widest mb-6 border border-brand-red/30">
                  Thành Viên Premium
                </span>
                
                <div className="w-full flex flex-col gap-4 mt-auto pt-6 border-t border-[#2a2a2a]">
                   <div className="flex justify-between items-center"><span className="text-white/40 text-xs uppercase tracking-wider">Thành viên từ</span><span className="text-white text-sm font-semibold">T1/2026</span></div>
                   <div className="flex justify-between items-center"><span className="text-white/40 text-xs uppercase tracking-wider">Giới tính</span><span className="text-white text-sm font-semibold">{currentUser?.gender === 'female' ? 'Nữ' : 'Nam'}</span></div>
                   <div className="flex justify-between items-center"><span className="text-white/40 text-xs uppercase tracking-wider">Số HLV</span><span className="text-white text-sm font-semibold">1</span></div>
                </div>
             </div>
          </div>

          {/* Cột 2 Hàng 1: Tổng buổi tập */}
          <div className="bg-[#161616] border border-[#2a2a2a] rounded-xl p-6 flex flex-col justify-center shadow-lg">
            <span className="text-white/40 text-[10px] uppercase tracking-widest mb-2 font-bold">Tổng Buổi Tập</span>
            <span className="text-[#e63333] font-display font-extrabold text-4xl">24</span>
          </div>

          {/* Cột 3 Hàng 1: HLV Đang Theo Học */}
          <div className="bg-[#161616] border border-[#2a2a2a] rounded-xl p-6 flex flex-col justify-center shadow-lg">
            <span className="text-white/40 text-[10px] uppercase tracking-widest mb-2 font-bold">HLV Đang Theo Học</span>
            <span className="text-white font-display font-bold text-2xl truncate">PT Test Account</span>
          </div>

          {/* Cột 2 Hàng 2: Body Stats */}
          <div className="bg-[#161616] border border-[#2a2a2a] rounded-xl p-6 shadow-lg">
            <span className="text-white/40 text-[10px] uppercase tracking-widest mb-5 block font-bold">Chỉ số Cơ Thể</span>
            <div className="grid grid-cols-2 gap-x-4 gap-y-5">
              <div><p className="text-white/30 text-[10px] uppercase font-semibold mb-1">Chiều cao</p><p className="text-white font-bold text-lg">{currentUser?.height || 175} <span className="text-xs text-white/40 font-normal">cm</span></p></div>
              <div><p className="text-white/30 text-[10px] uppercase font-semibold mb-1">Cân nặng</p><p className="text-white font-bold text-lg">{currentUser?.weight || 70} <span className="text-xs text-white/40 font-normal">kg</span></p></div>
              <div><p className="text-white/30 text-[10px] uppercase font-semibold mb-1">BMI</p><p className="text-[#e63333] font-bold text-lg">{currentUser?.height && currentUser?.weight ? (currentUser.weight / Math.pow(currentUser.height/100, 2)).toFixed(1) : '22.9'}</p></div>
              <div><p className="text-white/30 text-[10px] uppercase font-semibold mb-1">Mục tiêu</p><p className="text-white font-bold text-lg">{currentUser?.targetWeight || 65} <span className="text-xs text-white/40 font-normal">kg</span></p></div>
            </div>
          </div>

          {/* Cột 3 Hàng 2: Progress Ring */}
          <div className="bg-[#161616] border border-[#2a2a2a] rounded-xl p-6 flex items-center justify-between gap-4 shadow-lg">
            <div className="flex flex-col">
              <span className="text-white/40 text-[10px] uppercase tracking-widest font-bold mb-2">Tiến độ mục tiêu</span>
              <span className="text-[#e63333] font-display font-extrabold text-4xl">68%</span>
              <p className="text-white/40 text-[10px] mt-2 font-medium">Cố gắng thêm 5kg nữa nhé!</p>
            </div>
            <div className="relative w-24 h-24 flex-shrink-0">
               <svg viewBox="0 0 36 36" className="w-full h-full transform -rotate-90">
                 <circle cx="18" cy="18" r="15.915" fill="none" stroke="#2a2a2a" strokeWidth="4" />
                 <circle cx="18" cy="18" r="15.915" fill="none" stroke="#e63333" strokeWidth="4" strokeDasharray="68, 100" strokeLinecap="round" />
               </svg>
            </div>
          </div>

          {/* Hàng 3 Full Width: Tabs & Form */}
          <div className="lg:col-span-3 min-w-0 w-full bg-[#161616] border border-[#2a2a2a] rounded-xl overflow-hidden shadow-lg">
            {/* Tab Navigation */}
            <div className="flex border-b border-[#2a2a2a] overflow-x-auto w-full max-w-full scrollbar-none px-2 pt-2 bg-gradient-to-r from-[#121212] to-[#161616]">
              {tabsList.map((tab) => {
                const Icon = tab.icon;
                return (
                  <button
                    key={tab.id}
                    onClick={() => setActiveTab(tab.id)}
                    className={`flex items-center gap-2 px-6 py-4 text-[11px] font-bold uppercase tracking-widest whitespace-nowrap transition-all duration-200 cursor-pointer border-b-2 ${
                      activeTab === tab.id
                        ? 'text-[#e63333] border-[#e63333] bg-[#e63333]/10'
                        : 'text-white/40 border-transparent hover:text-white hover:border-white/20'
                    }`}
                  >
                    <Icon size={14} className={activeTab === tab.id ? 'text-[#e63333]' : 'text-white/40'} />
                    {tab.label}
                  </button>
                );
              })}
            </div>

            <div className="p-6 md:p-8">
            {/* Tab: Profile */}
            {activeTab === 'profile' && (
              <div className="animate-fade-in">
                <div className="flex items-center justify-between mb-6">
                  <h3 className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider">Thông Tin Cá Nhân</h3>
                  <button
                    onClick={() => { if (!editing) handleStartEdit(); else setEditing(false); }}
                    className={`flex items-center gap-2 text-xs uppercase tracking-widest cursor-pointer transition-colors duration-200 ${editing ? 'text-brand-red' : 'text-white/40 hover:text-white'}`}
                  >
                    <Edit3 size={14} />
                    {editing ? 'Đang Chỉnh Sửa...' : 'Chỉnh Sửa'}
                  </button>
                </div>

                <div className="grid md:grid-cols-2 gap-4">
                  {editing ? (
                    <>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Họ và Tên</label>
                        <input
                          type="text"
                          value={formFields.fullName}
                          onChange={(e) => setFormFields({ ...formFields, fullName: e.target.value })}
                          className="input-dark"
                        />
                      </div>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Email (Không thể thay đổi)</label>
                        <p className="text-white/40 text-sm px-4 py-3 bg-brand-surface border border-brand-border">{currentUser?.email}</p>
                      </div>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Số Điện Thoại</label>
                        <input
                          type="text"
                          value={formFields.phoneNumber}
                          onChange={(e) => setFormFields({ ...formFields, phoneNumber: e.target.value })}
                          className="input-dark"
                        />
                      </div>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Giới Tính</label>
                        <select
                          value={formFields.gender}
                          onChange={(e) => setFormFields({ ...formFields, gender: e.target.value })}
                          className="input-dark"
                        >
                          <option value="male">Nam</option>
                          <option value="female">Nữ</option>
                          <option value="other">Khác</option>
                        </select>
                      </div>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Chiều Cao (cm)</label>
                        <input
                          type="number"
                          value={formFields.height}
                          onChange={(e) => setFormFields({ ...formFields, height: Number(e.target.value) })}
                          className="input-dark"
                        />
                      </div>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Cân Nặng Hiện Tại (kg)</label>
                        <input
                          type="number"
                          value={formFields.weight}
                          onChange={(e) => setFormFields({ ...formFields, weight: Number(e.target.value) })}
                          className="input-dark"
                        />
                      </div>
                    </>
                  ) : (
                    <>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Họ và Tên</label>
                        <p className="text-white text-sm px-4 py-3 bg-brand-surface border border-brand-border">{currentUser?.fullName || ''}</p>
                      </div>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Email</label>
                        <p className="text-white text-sm px-4 py-3 bg-brand-surface border border-brand-border">{currentUser?.email || ''}</p>
                      </div>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Số Điện Thoại</label>
                        <p className="text-white text-sm px-4 py-3 bg-brand-surface border border-brand-border">{currentUser?.phoneNumber || '0901 234 567'}</p>
                      </div>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Giới Tính</label>
                        <p className="text-white text-sm px-4 py-3 bg-brand-surface border border-brand-border">
                          {currentUser?.gender === 'female' ? 'Nữ' : currentUser?.gender === 'other' ? 'Khác' : 'Nam'}
                        </p>
                      </div>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Chiều Cao (cm)</label>
                        <p className="text-white text-sm px-4 py-3 bg-brand-surface border border-brand-border">{currentUser?.height || '175'}</p>
                      </div>
                      <div>
                        <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Cân Nặng Hiện Tại (kg)</label>
                        <p className="text-white text-sm px-4 py-3 bg-brand-surface border border-brand-border">{currentUser?.weight || '74.8'}</p>
                      </div>
                    </>
                  )}
                </div>

                {editing && (
                  <div className="flex gap-3 mt-6">
                    <button onClick={handleSaveProfile} className="btn-primary text-xs py-3 px-8">Lưu Thay Đổi</button>
                    <button onClick={() => setEditing(false)} className="btn-secondary text-xs py-3 px-8">Hủy</button>
                  </div>
                )}
              </div>
            )}

            {/* Tab: Fitness */}
            {activeTab === 'fitness' && (
              <div className="animate-fade-in">
                <h3 className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider mb-8">Thể Trạng & Sức Khỏe</h3>

                {/* BMI + Key Stats */}
                <div className="grid md:grid-cols-3 gap-6 mb-8">
                  <div className="bg-brand-surface border border-brand-border p-6 flex flex-col items-center">
                    <BMIGauge value={latestScan.bmi} />
                  </div>

                  {[
                    { label: 'Cân Nặng', value: `${latestScan.weight}kg`, change: `${(latestScan.weight - firstScan.weight).toFixed(1)}kg`, improving: latestScan.weight < firstScan.weight },
                    { label: 'Mỡ Cơ Thể', value: `${latestScan.bodyFat}%`, change: `${(latestScan.bodyFat - firstScan.bodyFat).toFixed(1)}%`, improving: latestScan.bodyFat < firstScan.bodyFat },
                    { label: 'Khối Cơ', value: `${latestScan.muscle}kg`, change: `+${(latestScan.muscle - firstScan.muscle).toFixed(1)}kg`, improving: true },
                  ].slice(0, 2).map((s, i) => (
                    <div key={i} className="bg-brand-surface border border-brand-border p-6">
                      <p className="text-white/40 text-xs uppercase tracking-widest mb-4">{s.label}</p>
                      <p className="font-mono font-bold text-4xl text-white mb-2">{s.value}</p>
                      <div className={`flex items-center gap-1 text-xs font-semibold ${s.improving ? 'text-white' : 'text-brand-red'}`}>
                        {s.improving ? <CheckCircle size={12} /> : <AlertCircle size={12} />}
                        {s.change} kể từ T1/2026
                      </div>
                    </div>
                  ))}
                </div>

                {/* Progress bars */}
                <div className="bg-brand-surface border border-brand-border p-6">
                  <h4 className="font-montserrat font-bold text-lg text-white uppercase tracking-wider mb-6">Tiến Trình Theo Tháng</h4>
                  <div className="flex flex-col gap-5">
                    {SCAN_HISTORY.map((scan, i) => (
                      <div key={i}>
                        <div className="flex items-center justify-between mb-2">
                          <span className="text-white/50 text-xs uppercase tracking-widest">{scan.date}</span>
                          <div className="flex gap-4 text-xs">
                            <span className="text-white">{scan.weight}kg</span>
                            <span className="text-white/40">{scan.bodyFat}% mỡ</span>
                          </div>
                        </div>
                        <div className="progress-track">
                          <div
                            className="progress-fill"
                            style={{ width: `${100 - ((scan.weight - 60) / 30) * 100}%` }}
                          />
                        </div>
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            )}

            {/* Tab: Transactions */}
            {activeTab === 'transactions' && (
              <div className="animate-fade-in">
                <h3 className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider mb-8">Lịch Sử Giao Dịch</h3>

                {/* Summary cards */}
                <div className="grid grid-cols-3 gap-4 mb-8">
                  {[
                    { label: 'Tổng Chi', value: '₫15.12M' },
                    { label: 'Giao Dịch', value: '5' },
                    { label: 'Đang Active', value: '1' },
                  ].map((s, i) => (
                    <div key={i} className="bg-brand-surface border border-brand-border p-5 text-center">
                      <p className="text-white/30 text-xs uppercase tracking-widest mb-2">{s.label}</p>
                      <p className="font-mono font-semibold text-3xl text-white">{s.value}</p>
                    </div>
                  ))}
                </div>

                {/* Transaction table */}
                <div className="bg-brand-surface border border-brand-border overflow-x-auto w-full max-w-full">
                  <div className="grid grid-cols-5 px-6 py-3 border-b border-brand-border min-w-[600px]">
                    {['Mã GD', 'HLV', 'Buổi', 'Số Tiền', 'Trạng Thái'].map((h) => (
                      <span key={h} className="text-white/30 text-xs uppercase tracking-widest">{h}</span>
                    ))}
                  </div>
                  {TRANSACTIONS.map((tx) => (
                    <div key={tx.id} className="grid grid-cols-5 px-6 py-4 border-b border-brand-border last:border-0 hover:bg-white/[0.02] transition-colors duration-150 min-w-[600px]">
                      <span className="text-white/50 text-xs font-mono">{tx.id}</span>
                      <span className="text-white text-xs">{tx.pt}</span>
                      <span className="text-white/50 text-xs">{tx.sessions} buổi</span>
                      <span className="text-white text-xs font-semibold">{tx.amount.toLocaleString('vi-VN')}đ</span>
                      <StatusBadge status={tx.status} />
                    </div>
                  ))}
                </div>
              </div>
            )}



            {/* Tab: Change Password */}
            {activeTab === 'password' && (
              <div className="animate-fade-in">
                <h3 className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider mb-6">Đổi Mật Khẩu</h3>
                
                {pwSuccess && (
                  <div className="mb-6 border border-white bg-white/5 px-4 py-3 text-white text-sm rounded-xl">
                    {pwSuccess}
                  </div>
                )}

                {pwError && (
                  <div className="mb-6 border border-brand-red bg-brand-red/10 px-4 py-3 text-brand-red text-sm">
                    {pwError}
                  </div>
                )}

                <form onSubmit={handleChangePassword} className="max-w-md flex flex-col gap-5">
                  <div>
                    <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Mật khẩu hiện tại</label>
                    <input
                      type="password"
                      value={currentPassword}
                      onChange={(e) => setCurrentPassword(e.target.value)}
                      placeholder="••••••••"
                      required
                      className="input-dark"
                      disabled={pwLoading}
                    />
                  </div>

                  <div>
                    <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Mật khẩu mới</label>
                    <input
                      type="password"
                      value={newPassword}
                      onChange={(e) => setNewPassword(e.target.value)}
                      placeholder="Tối thiểu 8 ký tự (chứa chữ và số)"
                      required
                      className="input-dark"
                      disabled={pwLoading}
                    />
                  </div>

                  <div>
                    <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Xác nhận mật khẩu mới</label>
                    <input
                      type="password"
                      value={confirmPassword}
                      onChange={(e) => setConfirmPassword(e.target.value)}
                      placeholder="Nhập lại mật khẩu mới"
                      required
                      className="input-dark"
                      disabled={pwLoading}
                    />
                  </div>

                  <button
                    type="submit"
                    disabled={pwLoading}
                    className="btn-primary py-3 px-8 text-xs font-semibold uppercase tracking-wider justify-center"
                  >
                    {pwLoading ? (
                      <div className="flex items-center gap-1.5">
                        <Loader2 className="animate-spin text-white" size={14} />
                        ĐANG CẬP NHẬT...
                      </div>
                    ) : (
                      'CẬP NHẬT MẬT KHẨU'
                    )}
                  </button>
                </form>
              </div>
            )}
            </div>
          </div>
        </div>
      </div>

      {/* Mobile Bottom Nav */}
      <nav className="mobile-bottom-bar">
        {[
          { label: 'Trang Chủ', icon: <Home size={20} />, path: '/' },
          { label: 'Tìm PT', icon: <Search size={20} />, path: '/marketplace' },
          { label: 'Lịch', icon: <Calendar size={20} />, path: '/customer/bookings' },
          { label: 'Chat', icon: <MessageSquare size={20} />, path: '/customer/workspace' },
          { label: 'Hồ Sơ', icon: <User size={20} />, path: '/customer/profile' },
        ].map((tab, i) => (
          <Link key={i} to={tab.path} className="flex flex-col items-center gap-1 px-3 py-1 text-white/50 hover:text-brand-red transition-colors">
            <span className="flex items-center justify-center">{tab.icon}</span>
            <span className="text-[10px] uppercase tracking-widest mt-1">{tab.label}</span>
          </Link>
        ))}
      </nav>
    </div>
  );
};

export default ProfilePage;
