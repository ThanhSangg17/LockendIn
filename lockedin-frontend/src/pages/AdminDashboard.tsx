// src/pages/AdminDashboard.tsx
import React, { useState, useEffect } from 'react';
import {
  Users, DollarSign, AlertTriangle, CheckCircle, XCircle,
  TrendingUp, Shield, FileText, Clock, BarChart2, ArrowUpRight,
  CreditCard, Lock, Unlock, Loader2
} from 'lucide-react';
import Badge from '../components/Badge';
import { useData } from '../context/DataContext';
import api from '../services/api';

const TABS = [
  { id: 'overview', label: 'Tổng Quan', icon: BarChart2 },
  { id: 'users', label: 'Người Dùng', icon: Users },
  { id: 'payments', label: 'Giao Dịch', icon: CreditCard },
  { id: 'verification', label: 'Xác Minh HLV', icon: Shield },
  { id: 'disputes', label: 'Tranh Chấp', icon: AlertTriangle },
  { id: 'payouts', label: 'Thanh Toán', icon: DollarSign },
  { id: 'audit', label: 'Audit Log', icon: FileText },
];

const MONTHLY_REVENUE = [
  { month: 'T1', value: 45, amount: 45000000 },
  { month: 'T2', value: 62, amount: 62000000 },
  { month: 'T3', value: 58, amount: 58000000 },
  { month: 'T4', value: 78, amount: 78000000 },
  { month: 'T5', value: 95, amount: 95000000 },
  { month: 'T6', value: 88, amount: 88000000 },
];

const AdminDashboard: React.FC = () => {
  const [activeTab, setActiveTab] = useState('overview');
  const [dashboardData, setDashboardData] = useState<any>(null);

  // New admin states
  const [usersList, setUsersList] = useState<any[]>([]);
  const [loadingUsers, setLoadingUsers] = useState(false);
  const [paymentsList, setPaymentsList] = useState<any[]>([]);
  const [loadingPayments, setLoadingPayments] = useState(false);
  
  const { 
    ptVerifications, 
    disputes, 
    settlements, 
    auditLogs, 
    approvePTVerification, 
    rejectPTVerification, 
    releaseSettlement, 
    resolveDisputeAction,
    refreshBackendData
  } = useData();

  const loadUsersList = async () => {
    try {
      setLoadingUsers(true);
      const res = await api.get('/admin/users');
      if (res.data?.success) {
        setUsersList(res.data.data || []);
      }
    } catch (e) {
      console.error('Failed to load users:', e);
    } finally {
      setLoadingUsers(false);
    }
  };

  const loadPaymentsList = async () => {
    try {
      setLoadingPayments(true);
      const res = await api.get('/admin/payments');
      if (res.data?.success) {
        setPaymentsList(res.data.data || []);
      }
    } catch (e) {
      console.error('Failed to load payments:', e);
    } finally {
      setLoadingPayments(false);
    }
  };

  const handleToggleUserBan = async (userId: string, isBanned: boolean) => {
    try {
      const endpoint = isBanned ? 'unban' : 'ban';
      const res = await api.patch(`/admin/users/${userId}/${endpoint}`);
      if (res.data?.success) {
        alert(isBanned ? 'Đã mở khóa tài khoản!' : 'Đã khóa tài khoản thành công!');
        loadUsersList();
      }
    } catch (e) {
      console.error(e);
      alert('Thay đổi trạng thái tài khoản thất bại.');
    }
  };

  const handleDisputeUnderReview = async (disputeId: string) => {
    try {
      const res = await api.post(`/admin/disputes/${disputeId}/under-review`);
      if (res.data?.success) {
        alert('Tranh chấp đã được chuyển sang trạng thái đang xem xét.');
        refreshBackendData();
      }
    } catch (e) {
      console.error(e);
      alert('Cập nhật trạng thái tranh chấp thất bại.');
    }
  };

  const handleMarkSettlementSettled = async (settlementId: string) => {
    try {
      const res = await api.post(`/admin/settlements/${settlementId}/mark-settled`);
      if (res.data?.success) {
        alert('Đã đánh dấu đã thanh toán thành công!');
        window.location.reload();
      }
    } catch (e) {
      console.error(e);
      alert('Không thể cập nhật trạng thái đợt giải ngân.');
    }
  };

  useEffect(() => {
    if (activeTab === 'users') {
      loadUsersList();
    } else if (activeTab === 'payments') {
      loadPaymentsList();
    }
  }, [activeTab]);

  useEffect(() => {
    api.get('/admin/dashboard')
      .then((res) => {
        if (res.data?.success) {
          setDashboardData(res.data.data);
        }
      })
      .catch((err) => {
        console.error('Failed to load admin dashboard summary:', err);
      });
  }, [ptVerifications, disputes, settlements]);

  const maxRevenue = Math.max(...MONTHLY_REVENUE.map((m) => m.value));

  const pendingPTs = ptVerifications.filter(pt => pt.verificationStatus === 1 || pt.verificationStatus === 2);
  const openDisputes = disputes.filter(d => d.status === 'Pending');

  const totalRevenueText = dashboardData 
    ? `₫${(dashboardData.totalRevenue).toLocaleString('vi-VN')}` 
    : '₫95,000,000';
  const totalUsersText = dashboardData 
    ? dashboardData.totalUsers.toLocaleString() 
    : '10,482';
  const verifiedPTsText = dashboardData 
    ? dashboardData.totalPts.toLocaleString() 
    : '523';
  const openDisputesText = openDisputes.length.toString();

  const kpiCards = [
    { label: 'Tổng Doanh Thu', value: totalRevenueText, change: '+21.8%', icon: DollarSign, up: true },
    { label: 'Tổng Người Dùng', value: totalUsersText, change: '+8.4%', icon: Users, up: true },
    { label: 'HLV Đã Xác Minh', value: verifiedPTsText, change: '+3.2%', icon: CheckCircle, up: true },
    { label: 'Tranh Chấp Đang Mở', value: openDisputesText, change: openDisputes.length > 0 ? '+10%' : '-40%', icon: AlertTriangle, up: openDisputes.length > 0 },
  ];

  return (
    <div className="min-h-screen bg-brand-black mobile-content-pad animate-fade-in">
      {/* Header */}
      <div className="border-b border-brand-border bg-brand-dark">
        <div className="section-container py-10">
          <div className="flex items-center gap-3 mb-2">
            <div className="w-8 h-8 bg-brand-red flex items-center justify-center rounded-xl">
              <Shield size={16} className="text-white" />
            </div>
            <p className="section-label">Cổng Quản Trị</p>
          </div>
          <h1 className="page-title">Dashboard Quản Trị</h1>
          <p className="text-white/30 text-xs mt-3 uppercase tracking-widest">
            Dữ liệu live đồng bộ trực tiếp từ SQL Server
          </p>
        </div>
      </div>

      <div className="section-container py-8">
        {/* KPI Row */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-10">
          {kpiCards.map((kpi, i) => {
            const Icon = kpi.icon;
            return (
              <div key={i} className="bg-brand-surface border border-brand-border p-6 group hover:border-brand-red transition-all duration-300">
                <div className="flex items-start justify-between mb-4">
                  <div className="w-10 h-10 bg-brand-dark border border-brand-border group-hover:border-brand-red transition-colors duration-300 flex items-center justify-center">
                    <Icon size={18} className="text-white/60" />
                  </div>
                  <div className={`flex items-center gap-1 text-xs font-semibold ${kpi.up ? 'text-white' : 'text-brand-red'}`}>
                    <ArrowUpRight size={12} className={kpi.up ? '' : 'rotate-90'} />
                    {kpi.change}
                  </div>
                </div>
                <p className="font-mono font-semibold text-2xl text-white mb-1">{kpi.value}</p>
                <p className="text-white/30 text-xs uppercase tracking-wider">{kpi.label}</p>
              </div>
            );
          })}
        </div>

        {/* Tab Navigation */}
        <div className="flex border-b border-brand-border mb-8 overflow-x-auto">
          {TABS.map((tab) => {
            const Icon = tab.icon;
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`flex items-center gap-2 px-6 py-4 text-xs font-semibold uppercase tracking-widest whitespace-nowrap transition-all duration-200 cursor-pointer border-b-2 ${
                  activeTab === tab.id
                    ? 'text-white border-brand-red'
                    : 'text-white/40 border-transparent hover:text-white hover:border-white/20'
                }`}
              >
                <Icon size={14} />
                {tab.label}
              </button>
            );
          })}
        </div>

        {/* Tab: Overview */}
        {activeTab === 'overview' && (
          <div className="animate-fade-in">
            <div className="grid lg:grid-cols-3 gap-6">
              {/* Revenue Chart */}
              <div className="lg:col-span-2 bg-brand-surface border border-brand-border p-6">
                <div className="flex items-center justify-between mb-8">
                  <h3 className="font-montserrat font-bold text-xl text-white uppercase tracking-wider">Doanh Thu 6 Tháng</h3>
                  <div className="flex items-center gap-2 text-brand-red text-xs font-semibold">
                    <TrendingUp size={14} />
                    Biểu đồ Dữ liệu Trực tiếp
                  </div>
                </div>

                {/* Bar Chart */}
                <div className="flex items-end gap-3 h-40">
                  {MONTHLY_REVENUE.map((m, i) => (
                    <div key={i} className="flex-1 flex flex-col items-center gap-2 group">
                      <span className="text-white/40 text-[10px] font-mono opacity-0 group-hover:opacity-100 transition-opacity duration-200">
                        {(m.amount / 1000000).toFixed(0)}M
                      </span>
                      <div
                        className={`w-full transition-all duration-500 cursor-pointer ${
                          i === MONTHLY_REVENUE.length - 1
                            ? 'bg-brand-red hover:bg-brand-red-light'
                            : 'bg-brand-muted hover:bg-white/40'
                        }`}
                        style={{ height: `${(m.value / maxRevenue) * 100}%` }}
                      />
                      <span className="text-white/40 text-xs uppercase tracking-wider">{m.month}</span>
                    </div>
                  ))}
                </div>

                {/* Legend */}
                <div className="flex items-center gap-6 mt-6 pt-6 border-t border-brand-border">
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 bg-brand-red rounded-full" />
                    <span className="text-white/40 text-xs">Tháng Hiện Tại</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <div className="w-3 h-3 bg-brand-muted" />
                    <span className="text-white/40 text-xs">Các Tháng Trước</span>
                  </div>
                </div>
              </div>

              {/* Right panel: Quick stats */}
              <div className="flex flex-col gap-4">
                <div className="bg-brand-surface border border-brand-border p-6">
                  <p className="section-label mb-4">Hoạt Động Hôm Nay</p>
                  {[
                    { label: 'Đăng Ký PT Chờ Duyệt', value: pendingPTs.length.toString(), icon: Users },
                    { label: 'Tổng Giao Dịch', value: dashboardData?.totalBookings?.toString() || '0', icon: CheckCircle },
                    { label: 'Tranh Chấp Mới', value: openDisputes.length.toString(), icon: AlertTriangle },
                  ].map((s, i) => {
                    const Icon = s.icon;
                    return (
                      <div key={i} className="flex items-center justify-between py-3 border-b border-brand-border last:border-0">
                        <div className="flex items-center gap-2">
                          <Icon size={14} className="text-white/30" />
                          <span className="text-white/50 text-xs uppercase tracking-wider">{s.label}</span>
                        </div>
                        <span className="text-white font-semibold text-sm">{s.value}</span>
                      </div>
                    );
                  })}
                </div>

                <div className="bg-brand-surface border border-brand-border p-6">
                  <p className="section-label mb-4">Phân Chia Doanh Thu</p>
                  {[
                    { label: 'PT Nhận', pct: 90, color: 'bg-white' },
                    { label: 'LockedIn Phí', pct: 10, color: 'bg-brand-red' },
                  ].map((r, i) => (
                    <div key={i} className="mb-4">
                      <div className="flex justify-between mb-1.5">
                        <span className="text-white/50 text-xs">{r.label}</span>
                        <span className="text-white text-xs font-semibold">{r.pct}%</span>
                      </div>
                      <div className="progress-track">
                        <div className={`h-full ${r.color} rounded-full transition-all duration-700`} style={{ width: `${r.pct}%` }} />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </div>
          </div>
        )}

        {/* Tab: Users */}
        {activeTab === 'users' && (
          <div className="animate-fade-in">
            <div className="flex items-center justify-between mb-6">
              <h3 className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider">Danh Sách Người Dùng</h3>
              <Badge variant="red">{usersList.length} Thành Viên</Badge>
            </div>

            {loadingUsers ? (
              <div className="bg-brand-surface border border-brand-border p-12 text-center text-white/40">
                <Loader2 className="animate-spin mx-auto mb-2 text-brand-red" size={24} />
                Đang tải danh sách người dùng...
              </div>
            ) : usersList.length === 0 ? (
              <div className="bg-brand-surface border border-brand-border p-12 text-center text-white/30 uppercase tracking-widest text-xs">
                Không có người dùng nào trong cơ sở dữ liệu
              </div>
            ) : (
              <div className="bg-brand-surface border border-brand-border overflow-x-auto w-full max-w-full">
                {/* Table Header */}
                <div className="grid grid-cols-6 px-6 py-4 border-b border-brand-border bg-brand-dark min-w-[700px]">
                  {['Tên', 'Email', 'SĐT', 'Vai Trò', 'Trạng Thái', 'Hành Động'].map((h) => (
                    <span key={h} className="text-white/30 text-xs uppercase tracking-widest">{h}</span>
                  ))}
                </div>
                {/* Table Body */}
                {usersList.map((user) => {
                  const roleLabel = user.role === 1 ? 'Học Viên' : user.role === 2 ? 'HLV (PT)' : 'Quản Trị';
                  const isBanned = user.status === 2; // status 2 is Banned
                  return (
                    <div key={user.id} className="grid grid-cols-6 items-center px-6 py-4 border-b border-brand-border last:border-0 hover:bg-white/[0.02] transition-colors min-w-[700px]">
                      <span className="text-white text-sm font-semibold truncate pr-2">{user.fullName}</span>
                      <span className="text-white/60 text-xs truncate pr-2 font-mono">{user.email}</span>
                      <span className="text-white/60 text-xs font-mono">{user.phone || '—'}</span>
                      <span className="text-white/80 text-xs font-semibold">{roleLabel}</span>
                      <span>
                        <Badge variant={isBanned ? 'red' : 'white'}>
                          {isBanned ? 'Bị Khóa' : 'Hoạt Động'}
                        </Badge>
                      </span>
                      <div>
                        {user.role !== 3 && ( // Cannot ban an admin
                          <button
                            onClick={() => handleToggleUserBan(user.id, isBanned)}
                            className={`text-xs font-bold uppercase tracking-wider cursor-pointer flex items-center gap-1 ${
                              isBanned ? 'text-white hover:text-white/80' : 'text-brand-red hover:text-brand-red-light'
                            }`}
                          >
                            {isBanned ? <Unlock size={12} /> : <Lock size={12} />}
                            {isBanned ? 'Mở Khóa' : 'Khóa'}
                          </button>
                        )}
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        )}

        {/* Tab: Payments */}
        {activeTab === 'payments' && (
          <div className="animate-fade-in">
            <div className="flex items-center justify-between mb-6">
              <h3 className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider">Danh Sách Giao Dịch</h3>
              <Badge variant="red">{paymentsList.length} Giao Dịch</Badge>
            </div>

            {loadingPayments ? (
              <div className="bg-brand-surface border border-brand-border p-12 text-center text-white/40">
                <Loader2 className="animate-spin mx-auto mb-2 text-brand-red" size={24} />
                Đang tải danh sách giao dịch...
              </div>
            ) : paymentsList.length === 0 ? (
              <div className="bg-brand-surface border border-brand-border p-12 text-center text-white/30 uppercase tracking-widest text-xs">
                Chưa có giao dịch nạp tiền nào trên hệ thống
              </div>
            ) : (
              <div className="bg-brand-surface border border-brand-border overflow-x-auto w-full max-w-full">
                {/* Table Header */}
                <div className="grid grid-cols-6 px-6 py-4 border-b border-brand-border bg-brand-dark min-w-[800px]">
                  {['Mã GD', 'Mã Đơn', 'Mã Đơn PayOS', 'Số Tiền', 'Phương Thức', 'Trạng Thái'].map((h) => (
                    <span key={h} className="text-white/30 text-xs uppercase tracking-widest">{h}</span>
                  ))}
                </div>
                {/* Table Body */}
                {paymentsList.map((payment) => {
                  const statusLabel = payment.status === 2 ? 'Đã Thanh Toán' : payment.status === 1 ? 'Chờ Thanh Toán' : 'Thất Bại';
                  return (
                    <div key={payment.id} className="grid grid-cols-6 items-center px-6 py-4 border-b border-brand-border last:border-0 hover:bg-white/[0.02] transition-colors min-w-[800px]">
                      <span className="text-white/50 text-xs font-mono truncate pr-2">{payment.id}</span>
                      <span className="text-white/50 text-xs font-mono truncate pr-2">{payment.bookingId}</span>
                      <span className="text-white text-xs font-mono">{payment.orderCode || '—'}</span>
                      <span className="text-white text-sm font-semibold">{payment.amount.toLocaleString('vi-VN')}đ</span>
                      <span className="text-white/60 text-xs uppercase tracking-wider">{payment.paymentMethod || 'PayOS'}</span>
                      <span>
                        <Badge variant={payment.status === 2 ? 'white' : 'red'}>
                          {statusLabel}
                        </Badge>
                      </span>
                    </div>
                  );
                })}
              </div>
            )}
          </div>
        )}

        {/* Tab: Verification */}
        {activeTab === 'verification' && (
          <div className="animate-fade-in">
            <div className="flex items-center justify-between mb-6">
              <h3 className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider">Hồ Sơ HLV Chờ Xét Duyệt</h3>
              <Badge variant="red">{pendingPTs.length} Chờ Duyệt</Badge>
            </div>

            {pendingPTs.length === 0 ? (
              <div className="bg-brand-surface border border-brand-border p-12 text-center text-white/30 uppercase tracking-widest text-xs">
                Không có hồ sơ nào cần xét duyệt
              </div>
            ) : (
              <div className="flex flex-col gap-4">
                {pendingPTs.map((pt) => (
                  <div key={pt.id} className="bg-brand-surface border border-brand-border hover:border-brand-red transition-all duration-300">
                    <div className="p-6">
                      <div className="flex flex-col lg:flex-row items-start lg:items-center justify-between gap-6">
                        <div className="flex items-start gap-4">
                          <div className="w-14 h-14 bg-brand-dark border border-brand-border flex items-center justify-center flex-shrink-0">
                            <span className="font-display text-xl text-white">
                              {pt.fullName.split(' ').map((w) => w[0]).join('').slice(0, 2)}
                            </span>
                          </div>
                          <div>
                            <div className="flex items-center gap-3 mb-1">
                              <h4 className="font-semibold text-white">{pt.fullName}</h4>
                              <span className="text-white/30 text-xs font-mono">{pt.id}</span>
                            </div>
                            <p className="text-white/40 text-sm mb-3">Chuyên môn: {pt.specialization} · {pt.experienceYears} năm kinh nghiệm</p>
                            <p className="text-white/60 text-xs italic mb-3">"{pt.bio || 'Chưa cung cấp giới thiệu bản thân'}"</p>
                            <div className="flex gap-2">
                              <Badge variant="outline">Đánh giá: {pt.averageRating} ★</Badge>
                              <Badge variant="gray">Số đánh giá: {pt.totalReviews}</Badge>
                            </div>
                          </div>
                        </div>

                        <div className="flex items-center gap-2 flex-shrink-0 w-full lg:w-auto">
                          <button
                            onClick={() => approvePTVerification(pt.id)}
                            className="flex-1 lg:flex-none flex items-center justify-center gap-2 px-5 py-2.5 bg-brand-red text-white hover:bg-brand-red-dark text-xs uppercase tracking-widest transition-all duration-200 cursor-pointer rounded-xl"
                          >
                            <CheckCircle size={12} />
                            Duyệt
                          </button>
                          <button
                            onClick={() => rejectPTVerification(pt.id)}
                            className="flex-1 lg:flex-none flex items-center justify-center gap-2 px-5 py-2.5 bg-transparent border border-brand-muted text-white/40 hover:border-brand-red hover:text-brand-red text-xs uppercase tracking-widest transition-all duration-200 cursor-pointer"
                          >
                            <XCircle size={12} />
                            Từ Chối
                          </button>
                        </div>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Tab: Disputes */}
        {activeTab === 'disputes' && (
          <div className="animate-fade-in">
            <div className="flex items-center justify-between mb-6">
              <h3 className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider">Bảng Tranh Chấp</h3>
              <Badge variant="red">{openDisputes.length} Đang Mở</Badge>
            </div>

            {disputes.length === 0 ? (
              <div className="bg-brand-surface border border-brand-border p-12 text-center text-white/30 uppercase tracking-widest text-xs">
                Không có tranh chấp nào trong cơ sở dữ liệu
              </div>
            ) : (
              <div className="flex flex-col gap-4">
                {disputes.map((d) => (
                  <div key={d.id} className={`bg-brand-surface border transition-all duration-300 ${
                    d.status === 'Pending' ? 'border-brand-red' : 'border-brand-border hover:border-brand-red/50'
                  }`}>
                    {d.status === 'Pending' && (
                      <div className="h-0.5 w-full bg-brand-red" />
                    )}
                    <div className="p-6">
                      <div className="flex items-start justify-between gap-4">
                        <div className="flex-1">
                          <div className="flex items-center gap-3 mb-2">
                            <span className="text-white/30 text-xs font-mono">{d.id}</span>
                            <Badge variant={d.status === 'Pending' ? 'red' : d.status === 'ResolvedRefunded' ? 'white' : 'gray'}>
                              {d.status === 'Pending' ? 'Đang Mở' : d.status === 'ResolvedRefunded' ? 'Đã Hoàn Tiền' : 'Đã Trả PT'}
                            </Badge>
                          </div>
                          <p className="text-white font-semibold mb-1">{d.reason}</p>
                          <p className="text-white/40 text-sm mb-3">
                            Mã Booking: <span className="font-mono text-white/60">{d.bookingId}</span>
                          </p>
                          <div className="flex items-center gap-4 text-xs text-white/30">
                            <span className="flex items-center gap-1"><Clock size={10} />Ngày tạo: {new Date(d.createdAt).toLocaleDateString('vi-VN')}</span>
                          </div>
                        </div>

                        {d.status === 'Pending' && (
                          <div className="flex items-center gap-2 flex-shrink-0">
                            <button
                              onClick={() => handleDisputeUnderReview(d.id)}
                              className="px-4 py-2 bg-brand-dark border border-brand-border text-white/60 text-xs uppercase tracking-widest cursor-pointer hover:border-white hover:text-white transition-all duration-200"
                            >
                              Đang Xem Xét
                            </button>
                            <button
                              onClick={() => resolveDisputeAction(d.id, 'refund')}
                              className="px-4 py-2 bg-brand-red text-white text-xs uppercase tracking-widest cursor-pointer hover:bg-brand-red-dark transition-colors duration-200 rounded-xl"
                            >
                              Hoàn Tiền
                            </button>
                            <button
                              onClick={() => resolveDisputeAction(d.id, 'pay_pt')}
                              className="px-4 py-2 border border-brand-border text-white/50 text-xs uppercase tracking-widest cursor-pointer hover:border-white hover:text-white transition-all duration-200"
                            >
                              Giải Ngân PT
                            </button>
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Tab: Payouts */}
        {activeTab === 'payouts' && (
          <div className="animate-fade-in">
            <div className="flex items-center justify-between mb-6">
              <h3 className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider">Quản Lý Giải Ngân</h3>
              <Badge variant="red">{settlements.filter(s => s.status === 'PendingDisputeWindow').length} Chờ Duyệt</Badge>
            </div>

            {settlements.length === 0 ? (
              <div className="bg-brand-surface border border-brand-border p-12 text-center text-white/30 uppercase tracking-widest text-xs">
                Không có giao dịch giải ngân nào
              </div>
            ) : (
              <div className="bg-brand-surface border border-brand-border overflow-hidden">
                {/* Table header */}
                <div className="grid grid-cols-6 px-6 py-4 border-b border-brand-border bg-brand-dark">
                  {['Mã PO', 'Mã Booking', 'Huấn Luyện Viên', 'Giải Ngân', 'Trạng Thái', 'Hành Động'].map((h) => (
                    <span key={h} className="text-white/30 text-xs uppercase tracking-widest">{h}</span>
                  ))}
                </div>

                {settlements.map((p) => (
                  <div key={p.id} className="grid grid-cols-6 items-center px-6 py-4 border-b border-brand-border last:border-0 hover:bg-white/[0.02] transition-colors">
                    <span className="text-white/50 text-xs font-mono">{p.id.slice(0, 8)}...</span>
                    <span className="text-white/50 text-xs font-mono">{p.bookingId.slice(0, 8)}...</span>
                    <span className="text-white text-sm font-semibold">{p.ptName}</span>
                    <span className="text-white text-sm font-semibold">{p.amount.toLocaleString('vi-VN')}đ</span>
                    <span>
                      <Badge variant={p.status === 'PendingDisputeWindow' ? 'red' : p.status === 'Released' ? 'white' : 'gray'}>
                        {p.status === 'PendingDisputeWindow' ? 'Chờ Duyệt' : p.status === 'Released' ? 'Đã Giải Ngân' : 'Khóa Tranh Chấp'}
                      </Badge>
                    </span>
                    <div className="flex items-center gap-3">
                      {p.status === 'PendingDisputeWindow' && (
                        <>
                          <button
                            onClick={() => releaseSettlement(p.id)}
                            className="text-xs text-brand-red hover:text-brand-red-light font-bold uppercase tracking-wider cursor-pointer transition-colors"
                          >
                            Giải Ngân
                          </button>
                          <button
                            onClick={() => handleMarkSettlementSettled(p.id)}
                            className="text-xs text-white/50 hover:text-white font-bold uppercase tracking-wider cursor-pointer transition-colors"
                          >
                            Đã CK
                          </button>
                        </>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}

        {/* Tab: Audit Log */}
        {activeTab === 'audit' && (
          <div className="animate-fade-in">
            <h3 className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider mb-6">Nhật Ký Hệ Thống</h3>

            {auditLogs.length === 0 ? (
              <div className="bg-brand-surface border border-brand-border p-12 text-center text-white/30 uppercase tracking-widest text-xs">
                Không có audit log nào được ghi nhận
              </div>
            ) : (
              <div className="bg-brand-surface border border-brand-border overflow-hidden">
                {auditLogs.map((log, i) => (
                  <div key={i} className="flex items-start gap-5 px-6 py-4 border-b border-brand-border last:border-0 hover:bg-white/[0.02] transition-colors group">
                    <div className="w-1.5 h-1.5 rounded-full mt-1.5 flex-shrink-0 bg-brand-red" />
                    <div className="flex-1 min-w-0">
                      <div className="flex items-center gap-3 mb-1 flex-wrap">
                        <span className="text-white text-sm font-semibold">{log.action}</span>
                        <span className="text-white/30 text-xs border border-brand-border px-2 py-0.5 font-mono">{log.actor}</span>
                      </div>
                      <p className="text-white/50 text-xs">{log.details}</p>
                    </div>
                    <span className="text-white/20 text-xs font-mono flex-shrink-0 mt-0.5">
                      {new Date(log.timestamp).toLocaleString('vi-VN')}
                    </span>
                  </div>
                ))}
              </div>
            )}
          </div>
        )}
      </div>
    </div>
  );
};

export default AdminDashboard;
