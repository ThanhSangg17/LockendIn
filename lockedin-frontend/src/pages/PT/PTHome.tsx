import React from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Calendar, Users, User, DollarSign, Award, BookOpen, AlertCircle, ArrowRight, CheckCircle, XCircle } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { useData } from '../../context/DataContext';

const PTHome: React.FC = () => {
  const { currentUser } = useAuth();
  const { bookings, workspaces, packages, acceptBooking, rejectBooking } = useData();
  const navigate = useNavigate();

  // Filter items specifically for this PT
  const myBookings = bookings || [];
  const myWorkspaces = workspaces || [];
  const myPackages = packages || [];

  // Calculate stats
  const activeBookings = myBookings.filter(b => b.status === 'Active');
  const pendingBookings = myBookings.filter(b => b.status === 'PaidPendingAcceptance');
  
  // Total Revenue (completed and active bookings)
  const totalRevenue = myBookings
    .filter(b => b.status === 'Active' || b.status === 'Completed')
    .reduce((sum, b) => sum + b.price, 0);

  const handleAccept = async (bookingId: string) => {
    if (!window.confirm('Bạn có đồng ý nhận học viên này và bắt đầu khóa tập không?')) return;
    try {
      await acceptBooking(bookingId);
      alert('Đã chấp nhận yêu cầu đặt lịch thành công!');
    } catch (e) {
      console.error('Failed to accept booking:', e);
    }
  };

  const handleReject = async (bookingId: string) => {
    if (!window.confirm('Bạn có chắc chắn muốn từ chối yêu cầu đặt lịch này không? Tiền sẽ được hoàn trả trực tiếp cho học viên.')) return;
    try {
      await rejectBooking(bookingId);
      alert('Đã từ chối yêu cầu đặt lịch.');
    } catch (e) {
      console.error('Failed to reject booking:', e);
    }
  };

  return (
    <div className="min-h-screen bg-brand-black pb-16">
      {/* ─── BANNER CHÀO MỪNG ─── */}
      <div className="relative overflow-hidden bg-brand-dark border-b border-brand-border py-12">
        <div className="absolute inset-0 opacity-5" style={{
          backgroundImage: `linear-gradient(#fff 1px, transparent 1px), linear-gradient(90deg, #fff 1px, transparent 1px)`,
          backgroundSize: '40px 40px',
        }} />
        <div className="section-container relative z-10">
          <span className="section-label mb-2">Trang Chủ HLV</span>
          <h1 className="text-4xl font-display uppercase tracking-widest text-white">
            Chào mừng quay trở lại, <span className="text-brand-red">HLV {currentUser?.fullName}</span>!
          </h1>
          <p className="text-white/50 text-sm mt-2 max-w-2xl font-light">
            Nơi quản lý tiến trình giảng dạy, thiết lập các gói tập luyện, ký quỹ an toàn và tương tác trực tiếp với các học viên của bạn.
          </p>
        </div>
      </div>

      <div className="section-container mt-10">
        {/* ─── CHỈ SỐ THỐNG KÊ ─── */}
        <h2 className="text-xs font-display uppercase tracking-widest text-white/50 mb-4">Chỉ Số Hiệu Suất Của Bạn</h2>
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 mb-10">
          {[
            { 
              label: 'Tổng Doanh Thu', 
              value: `${(totalRevenue).toLocaleString('vi-VN')} đ`, 
              icon: <DollarSign size={20} className="text-brand-red" />,
              desc: 'Giao dịch thành công & ký quỹ'
            },
            { 
              label: 'Học Viên Đang Dạy', 
              value: activeBookings.length, 
              icon: <Users size={20} className="text-brand-red" />,
              desc: 'Số lớp học đang hoạt động'
            },
            { 
              label: 'Yêu Cầu Mới', 
              value: pendingBookings.length, 
              icon: <AlertCircle size={20} className="text-brand-red" />,
              desc: 'Chờ phê duyệt lịch đặt'
            },
            { 
              label: 'Gói Dịch Vụ', 
              value: myPackages.length, 
              icon: <Award size={20} className="text-brand-red" />,
              desc: 'Gói tập luyện đang kích hoạt'
            },
          ].map((stat, i) => (
            <div key={i} className="bg-brand-surface border border-brand-border p-5 flex flex-col justify-between transition-all duration-200 hover:border-brand-red/50">
              <div className="flex items-center justify-between mb-3">
                <span className="text-white/40 text-xs uppercase tracking-widest font-semibold">{stat.label}</span>
                {stat.icon}
              </div>
              <div>
                <p className="font-display font-extrabold text-2xl text-white mb-1">{stat.value}</p>
                <p className="text-white/30 text-[10px]">{stat.desc}</p>
              </div>
            </div>
          ))}
        </div>

        {/* ─── TRUY CẬP NHANH ─── */}
        <h2 className="text-xs font-display uppercase tracking-widest text-white/50 mb-4">Lối Tắt Quản Lý Nhanh</h2>
        <div className="grid md:grid-cols-4 gap-4 mb-10">
          {[
            {
              title: 'Gói Dịch Vụ',
              desc: 'Thiết lập & chỉnh sửa các gói tập luyện, buổi tập của bạn.',
              path: '/pt/packages',
              btnText: 'Quản lý Gói',
              icon: <Award size={24} className="text-brand-red" />
            },
            {
              title: 'Quản Lý Lịch Đặt',
              desc: 'Duyệt các yêu cầu tập luyện mới hoặc đã hoàn thành từ học viên.',
              path: '/pt/bookings',
              btnText: 'Xem Lịch Đặt',
              icon: <Calendar size={24} className="text-brand-red" />
            },
            {
              title: 'Lớp Học Trực Tuyến',
              desc: 'Nhắn tin, lên thực đơn AI, ghi nhận số buổi tập trực tiếp với học viên.',
              path: '/pt/workspace',
              btnText: 'Vào Lớp Học',
              icon: <BookOpen size={24} className="text-brand-red" />
            },
            {
              title: 'Hồ Sơ HLV',
              desc: 'Cập nhật tiểu sử, năm kinh nghiệm, avatar và thông tin cá nhân.',
              path: '/pt/profile',
              btnText: 'Sửa Hồ Sơ',
              icon: <User size={24} className="text-brand-red" />
            }
          ].map((item, i) => (
            <div key={i} className="bg-brand-surface border border-brand-border p-6 flex flex-col justify-between transition-all duration-300 hover:-translate-y-1 hover:bg-brand-dark/50">
              <div>
                <div className="mb-4">{item.icon}</div>
                <h3 className="text-white font-bold text-lg mb-2">{item.title}</h3>
                <p className="text-white/40 text-xs leading-relaxed mb-6">{item.desc}</p>
              </div>
              <Link to={item.path} className="btn-secondary text-xs py-2 w-full text-center flex items-center justify-center gap-2">
                {item.btnText}
                <ArrowRight size={14} />
              </Link>
            </div>
          ))}
        </div>

        <div className="grid lg:grid-cols-3 gap-8">
          {/* ─── LỚP HỌC ĐANG HOẠT ĐỘNG ─── */}
          <div className="lg:col-span-2">
            <h2 className="text-xs font-display uppercase tracking-widest text-white/50 mb-4">Lớp Học Đang Diễn Ra ({activeBookings.length})</h2>
            
            {activeBookings.length === 0 ? (
              <div className="bg-brand-surface border border-brand-border p-8 text-center text-white/30 text-sm">
                Bạn chưa có học viên nào đang tham gia khóa học trực tuyến. 
                Hãy đợi học viên đặt lịch và chấp nhận yêu cầu của họ để bắt đầu!
              </div>
            ) : (
              <div className="flex flex-col gap-4">
                {activeBookings.map((b) => {
                  const ws = myWorkspaces.find(w => w.bookingId === b.id);
                  const completedSessions = ws ? ws.sessionsCompleted : 0;
                  const totalSessions = ws ? ws.sessionsTotal : b.sessionsCount;
                  const progressPct = totalSessions > 0 ? Math.min(100, Math.round((completedSessions / totalSessions) * 100)) : 0;
                  
                  return (
                    <div key={b.id} className="bg-brand-surface border border-brand-border p-5 flex flex-col md:flex-row md:items-center justify-between gap-4">
                      <div className="flex-1">
                        <div className="flex items-center gap-2 mb-2">
                          <span className="text-xs font-bold text-brand-red bg-brand-red/10 px-2 py-0.5 uppercase">ĐANG HỌC</span>
                          <span className="text-white/30 text-xs">Mã lớp: {b.id.substring(0,8)}...</span>
                        </div>
                        <h3 className="text-white font-bold text-lg">{b.customerName}</h3>
                        <p className="text-white/50 text-xs mt-1">Gói: <span className="text-white font-medium">{b.packageName}</span> ({b.sessionsCount} buổi)</p>
                        
                        {/* Progress Bar */}
                        <div className="mt-4 max-w-xs">
                          <div className="flex justify-between text-[10px] text-white/40 mb-1">
                            <span>Tiến độ học tập</span>
                            <span>{completedSessions}/{totalSessions} buổi</span>
                          </div>
                          <div className="w-full bg-white/10 h-1.5 rounded-full overflow-hidden">
                            <div 
                              className="bg-brand-red h-full transition-all duration-500 rounded-xl" 
                              style={{ width: `${progressPct}%` }}
                            />
                          </div>
                        </div>
                      </div>
                      
                      <div className="flex items-center gap-2">
                        <button 
                          onClick={() => navigate(`/pt/workspace?bookingId=${b.id}`)} 
                          className="btn-primary text-xs py-2.5 px-5"
                        >
                          Vào Dạy Học
                        </button>
                      </div>
                    </div>
                  );
                })}
              </div>
            )}
          </div>

          {/* ─── YÊU CẦU MỚI CHỜ DUYỆT ─── */}
          <div>
            <h2 className="text-xs font-display uppercase tracking-widest text-white/50 mb-4">Yêu Cầu Đang Chờ ({pendingBookings.length})</h2>
            
            {pendingBookings.length === 0 ? (
              <div className="bg-brand-surface border border-brand-border p-6 text-center text-white/30 text-xs">
                Hiện tại không có yêu cầu đặt lịch mới nào cần phê duyệt.
              </div>
            ) : (
              <div className="flex flex-col gap-3">
                {pendingBookings.map((b) => (
                  <div key={b.id} className="bg-brand-surface border border-brand-border p-4 flex flex-col gap-3">
                    <div>
                      <p className="text-white/40 text-[10px] uppercase tracking-wider font-semibold">Khách Hàng</p>
                      <h4 className="text-white font-bold text-sm">{b.customerName}</h4>
                    </div>
                    <div>
                      <p className="text-white/40 text-[10px] uppercase tracking-wider font-semibold">Khóa Tập</p>
                      <p className="text-white text-xs font-semibold">{b.packageName}</p>
                      <p className="text-white/50 text-[11px] mt-0.5">{b.sessionsCount} buổi • {b.price.toLocaleString('vi-VN')} đ</p>
                    </div>

                    <div className="grid grid-cols-2 gap-2 mt-2">
                      <button 
                        onClick={() => handleAccept(b.id)}
                        className="btn-primary text-xs py-2 flex items-center justify-center gap-1 cursor-pointer"
                      >
                        <CheckCircle size={14} />
                        Đồng Ý
                      </button>
                      <button 
                        onClick={() => handleReject(b.id)}
                        className="btn-secondary text-xs py-2 flex items-center justify-center gap-1 cursor-pointer"
                      >
                        <XCircle size={14} />
                        Từ Chối
                      </button>
                    </div>
                  </div>
                ))}
              </div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PTHome;
