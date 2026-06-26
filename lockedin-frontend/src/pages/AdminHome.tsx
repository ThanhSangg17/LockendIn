// src/pages/AdminHome.tsx
import React, { useState, useEffect } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Shield, AlertTriangle, ArrowRight, CheckCircle, XCircle, Activity, Server, Cpu, Database, RefreshCw } from 'lucide-react';
import { useData } from '../context/DataContext';
import api from '../services/api';
import logoImg from '../assets/logo.png';

const AdminHome: React.FC = () => {
  const { ptVerifications, disputes, auditLogs, approvePTVerification, rejectPTVerification } = useData();
  const [dashboardData, setDashboardData] = useState<any>(null);
  const [refreshing, setRefreshing] = useState(false);
  const navigate = useNavigate();

  const loadData = () => {
    setRefreshing(true);
    api.get('/admin/dashboard')
      .then((res) => {
        if (res.data?.success) {
          setDashboardData(res.data.data);
        }
      })
      .catch((err) => {
        console.error('Failed to load admin dashboard summary:', err);
      })
      .finally(() => {
        setRefreshing(false);
      });
  };

  useEffect(() => {
    loadData();
  }, [ptVerifications, disputes]);

  const pendingPTs = ptVerifications.filter(pt => pt.verificationStatus === 1 || pt.verificationStatus === 2);
  const openDisputes = disputes.filter(d => d.status === 'Pending');

  const totalRevenue = dashboardData ? dashboardData.totalRevenue : 95000000;
  const totalUsers = dashboardData ? dashboardData.totalUsers : 10482;

  const handleApprovePT = async (ptId: string) => {
    if (!window.confirm('Xác nhận phê duyệt HLV này hoạt động trên hệ thống?')) return;
    try {
      await approvePTVerification(ptId);
      alert('Đã phê duyệt HLV thành công!');
    } catch (e) {
      console.error(e);
    }
  };

  const handleRejectPT = async (ptId: string) => {
    if (!window.confirm('Từ chối hồ sơ xác minh của HLV này?')) return;
    try {
      await rejectPTVerification(ptId);
      alert('Đã từ chối hồ sơ xác minh.');
    } catch (e) {
      console.error(e);
    }
  };

  return (
    <div className="min-h-screen bg-brand-black pb-16 text-white mobile-content-pad">
      {/* ─── HỆ THỐNG GIAO DIỆN BENTO GRID PREMIUM ─── */}
      <div className="section-container pt-8">
        
        {/* TOP STATUS BAR */}
        <div className="flex justify-between items-center mb-8 border-b border-brand-border pb-4">
          <div className="flex items-center gap-3">
            <span className="h-3 w-3 rounded-full bg-brand-red animate-pulse" />
            <div className="flex items-center gap-2">
              <img src={logoImg} alt="LockedIn" className="h-5 w-auto object-contain" />
              <span className="font-display text-sm uppercase tracking-widest font-black text-white pt-0.5">
                ADMIN PANEL
              </span>
            </div>
          </div>
          <button 
            onClick={loadData} 
            className="flex items-center gap-1.5 text-xs text-white/50 hover:text-white transition-colors cursor-pointer"
            title="Làm mới dữ liệu"
          >
            <RefreshCw size={14} className={refreshing ? 'animate-spin' : ''} />
            <span>{refreshing ? 'Đang tải...' : 'Làm mới'}</span>
          </button>
        </div>

        {/* BENTO GRID LAYOUT */}
        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          
          {/* 1. WELCOME & BRAND BANNER CARD (Large 2-column Block) */}
          <div className="lg:col-span-2 bg-gradient-to-r from-brand-red via-[#ad0505] to-[#800000] p-8 relative overflow-hidden group shadow-lg rounded-2xl">
            {/* Abstract visual background circles */}
            <div className="absolute -right-10 -bottom-10 w-48 h-48 bg-white/5 rounded-full blur-xl group-hover:scale-110 transition-transform duration-500" />
            <div className="absolute right-1/4 -top-10 w-32 h-32 bg-black/10 rounded-full blur-lg" />
            
            <div className="relative z-10 flex flex-col justify-between h-full min-h-[160px]">
              <div>
                <span className="text-xs text-white/70 uppercase tracking-widest font-bold font-mono">Bảng Điều Hợp Hệ Thống</span>
                <h2 className="text-3xl font-display font-black tracking-wide uppercase mt-2 text-white">
                  ĐIỀU HÀNH HỆ THỐNG THỜI GIAN THỰC
                </h2>
                <p className="text-white/85 text-xs mt-2 max-w-xl font-light leading-relaxed">
                  LockedIn quản lý tối ưu luồng Escrow, xác minh danh tính HLV và các hoạt động nhật ký hệ thống. Đảm bảo toàn bộ quy trình ký quỹ và giảng dạy luôn minh bạch, an toàn.
                </p>
              </div>
              <div className="flex items-center gap-4 mt-6 pt-4 border-t border-white/10">
                <div className="text-xs text-white/70 font-mono">
                  Environment: <span className="text-white font-bold">DEVELOPMENT</span>
                </div>
                <div className="h-3 w-px bg-white/20" />
                <div className="text-xs text-white/70 font-mono">
                  Gateway: <span className="text-white font-bold">localhost:5122</span>
                </div>
              </div>
            </div>
          </div>

          {/* 2. SYSTEM HARDWARE & MONITOR GATE (1-column Block) */}
          <div className="bg-brand-surface border border-brand-border p-6 flex flex-col justify-between shadow-md">
            <div>
              <div className="flex items-center justify-between mb-4 border-b border-brand-border pb-3">
                <span className="text-white/40 text-xs uppercase tracking-widest font-bold flex items-center gap-1.5">
                  <Server size={14} className="text-brand-red" /> TÌNH TRẠNG MÁY CHỦ
                </span>
                <span className="h-2 w-2 rounded-full bg-emerald-400 animate-pulse" />
              </div>
              
              <div className="flex flex-col gap-3">
                <div className="flex justify-between items-center text-xs">
                  <span className="text-white/50 flex items-center gap-1.5"><Database size={12} className="text-white/40" /> SQL Server:</span>
                  <span className="text-white font-mono font-semibold">Active (SQLEXPRESS)</span>
                </div>
                <div className="flex justify-between items-center text-xs">
                  <span className="text-white/50 flex items-center gap-1.5"><Cpu size={12} className="text-white/40" /> API Server:</span>
                  <span className="text-white font-mono font-semibold">Online (HTTP 5122)</span>
                </div>
                <div className="flex justify-between items-center text-xs">
                  <span className="text-white/50 flex items-center gap-1.5"><Activity size={12} className="text-white/40" /> Latency:</span>
                  <span className="text-brand-red font-mono font-semibold">12ms (Localhost)</span>
                </div>
              </div>
            </div>
            
            <div className="mt-6 pt-3 border-t border-brand-border flex items-center justify-between">
              <span className="text-[10px] text-white/30 uppercase font-mono">System Sec. Level</span>
              <span className="text-[10px] text-brand-red font-bold font-mono bg-brand-red/10 border border-brand-red/20 px-2 py-0.5 rounded">ROOT_ACCESS</span>
            </div>
          </div>

          {/* 3. MULTI-DIVIDED STATS PANEL (Full-width 3-column Block) */}
          <div className="lg:col-span-3 bg-brand-surface border border-brand-border shadow-sm rounded-2xl">
            <div className="grid grid-cols-2 lg:grid-cols-4 divide-y lg:divide-y-0 lg:divide-x divide-brand-border">
              {[
                { 
                  label: 'DOANH THU KÝ QUỸ', 
                  value: `${(totalRevenue).toLocaleString('vi-VN')} đ`, 
                  desc: 'Tổng khối lượng lớp học giao dịch'
                },
                { 
                  label: 'TỔNG THÀNH VIÊN', 
                  value: totalUsers, 
                  desc: 'Học viên & HLV trên hệ thống'
                },
                { 
                  label: 'HỒ SƠ CHỜ DUYỆT', 
                  value: pendingPTs.length, 
                  desc: 'HLV đang chờ kiểm tra CCCD'
                },
                { 
                  label: 'TRANH CHẤP CHỜ GIẢI QUYẾT', 
                  value: openDisputes.length, 
                  desc: 'Khiếu nại đang chờ phân xử'
                },
              ].map((stat, i) => (
                <div key={i} className="p-6 flex flex-col justify-between min-h-[110px]">
                  <span className="text-white/40 text-[10px] uppercase tracking-widest font-bold block mb-2">{stat.label}</span>
                  <div>
                    <span className="font-display font-black text-2xl text-white block mb-0.5">{stat.value}</span>
                    <span className="text-white/30 text-[9px] block">{stat.desc}</span>
                  </div>
                </div>
              ))}
            </div>
          </div>

          {/* 4. VERIFICATION QUEUE (Large 2-column block for main workflow list) */}
          <div className="lg:col-span-2 flex flex-col gap-6">
            
            {/* PT VERIFICATIONS */}
            <div className="bg-brand-surface border border-brand-border p-6 shadow-sm">
              <div className="flex items-center justify-between mb-4 border-b border-brand-border pb-3">
                <h3 className="text-white font-display font-bold text-sm uppercase tracking-wider flex items-center gap-2">
                  <Shield size={16} className="text-brand-red" /> Hồ Sơ PT Mới Chờ Duyệt ({pendingPTs.length})
                </h3>
                <Link to="/admin/dashboard" className="text-white/40 hover:text-brand-red text-xs transition-colors flex items-center gap-1 font-semibold">
                  Xem tất cả
                  <ArrowRight size={12} />
                </Link>
              </div>

              {pendingPTs.length === 0 ? (
                <div className="py-10 text-center text-white/30 text-sm">
                  Không có đơn xét duyệt nào đang chờ phê duyệt.
                </div>
              ) : (
                <div className="flex flex-col gap-4">
                  {pendingPTs.map((pt) => (
                    <div key={pt.id} className="bg-brand-black/50 border border-brand-border p-4 hover:border-brand-red/30 transition-colors">
                      <div className="flex justify-between items-start mb-2">
                        <div>
                          <h4 className="text-white font-bold text-sm">{pt.fullName}</h4>
                          <p className="text-white/40 text-[11px] mt-0.5">Kinh nghiệm: <span className="text-white">{pt.experienceYears} năm</span> | Chuyên ngành: <span className="text-white">{pt.specialization}</span></p>
                        </div>
                        <span className="text-[9px] font-bold text-brand-red bg-brand-red/10 border border-brand-red/20 px-2 py-0.5 rounded font-mono uppercase">Xác minh</span>
                      </div>
                      <p className="text-white/60 text-xs italic bg-brand-surface/40 p-2.5 border-l border-brand-red">"{pt.bio}"</p>

                      <div className="flex gap-2 mt-3">
                        <button 
                          onClick={() => handleApprovePT(pt.id)}
                          className="bg-brand-red hover:bg-brand-red/90 text-white text-xs py-1.5 px-4 font-bold flex items-center gap-1 cursor-pointer rounded-xl"
                        >
                          <CheckCircle size={12} />
                          Duyệt
                        </button>
                        <button 
                          onClick={() => handleRejectPT(pt.id)}
                          className="border border-white/20 hover:border-brand-red hover:text-brand-red text-white/80 text-xs py-1.5 px-4 font-bold flex items-center gap-1 cursor-pointer"
                        >
                          <XCircle size={12} />
                          Từ Chối
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              )}
            </div>

            {/* TRANH CHẤP ESCROW */}
            <div className="bg-brand-surface border border-brand-border p-6 shadow-sm">
              <div className="flex items-center justify-between mb-4 border-b border-brand-border pb-3">
                <h3 className="text-white font-display font-bold text-sm uppercase tracking-wider flex items-center gap-2">
                  <AlertTriangle size={16} className="text-brand-red" /> Khiếu Nại Tranh Chấp Đang Mở ({openDisputes.length})
                </h3>
              </div>

              {openDisputes.length === 0 ? (
                <div className="py-8 text-center text-white/30 text-xs font-light">
                  Hiện không có tranh chấp khiếu nại nào cần giải quyết.
                </div>
              ) : (
                <div className="flex flex-col gap-3">
                  {openDisputes.map((d) => (
                    <div key={d.id} className="bg-brand-black/50 border border-brand-border p-4">
                      <div className="flex justify-between items-start mb-2">
                        <div>
                          <h4 className="text-white font-bold text-sm">Khách hàng: {d.customerName}</h4>
                          <p className="text-white/40 text-[11px]">HLV phụ trách: <span className="text-white">{d.ptName}</span></p>
                        </div>
                        <span className="text-[9px] font-bold text-amber-500 bg-amber-500/10 border border-amber-500/20 px-2 py-0.5 rounded font-mono uppercase rounded-xl">Dispute</span>
                      </div>
                      <div className="text-xs bg-brand-surface/40 p-2 border-l border-brand-red">
                        <p className="text-white/70 font-semibold">Lý do khiếu nại:</p>
                        <p className="text-white/50 mt-0.5">{d.reason}</p>
                      </div>
                      <button 
                        onClick={() => navigate('/admin/dashboard')} 
                        className="btn-secondary text-[11px] py-1.5 mt-3 w-full flex items-center justify-center gap-1 cursor-pointer"
                      >
                        Đến Cổng Trọng Tài Phân Xử
                        <ArrowRight size={12} />
                      </button>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* 5. AUDIT LOG REALTIME STREAM (1-column block for logs) */}
          <div className="bg-brand-surface border border-brand-border p-6 shadow-sm flex flex-col justify-between">
            <div>
              <div className="flex items-center justify-between mb-4 border-b border-brand-border pb-3">
                <h3 className="text-white font-display font-bold text-sm uppercase tracking-wider flex items-center gap-2">
                  <Activity size={16} className="text-brand-red" /> Nhật Ký Hệ Thống
                </h3>
              </div>

              <div className="flex flex-col gap-4 max-h-[420px] overflow-y-auto pr-1">
                {auditLogs.length === 0 ? (
                  <div className="py-12 text-center text-white/30 text-xs">
                    Chưa có nhật ký hoạt động nào.
                  </div>
                ) : (
                  auditLogs.slice(0, 8).map((log) => (
                    <div key={log.id} className="border-b border-brand-border/40 pb-3 last:border-b-0 last:pb-0">
                      <div className="flex justify-between items-start gap-2 mb-1">
                        <span className="text-xs font-bold text-brand-red font-mono">{log.action}</span>
                        <span className="text-[9px] text-white/40 font-mono">
                          {new Date(log.timestamp).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                        </span>
                      </div>
                      <p className="text-[11px] text-white/60 leading-relaxed font-light">{log.details}</p>
                      <p className="text-[9px] text-white/30 mt-0.5 font-mono">User: {log.actor.substring(0,8)}...</p>
                    </div>
                  ))
                )}
              </div>
            </div>
            
            <Link 
              to="/admin/dashboard" 
              className="mt-6 btn-secondary text-xs py-2 w-full text-center flex items-center justify-center gap-2 cursor-pointer"
            >
              Mở Phân Hệ Logs Toàn Diện
              <ArrowRight size={14} />
            </Link>
          </div>

        </div>
      </div>
    </div>
  );
};

export default AdminHome;
