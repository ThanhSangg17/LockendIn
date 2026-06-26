// src/pages/CheckoutSim.tsx
import React, { useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Shield, CheckCircle, Lock, ArrowLeft, X, ExternalLink, CreditCard } from 'lucide-react';
import { useData } from '../context/DataContext';
import api from '../services/api';

const PAYMENT_METHODS = [
  { id: 'card', label: 'Thẻ Tín Dụng / Ghi Nợ', logo: '/visa_logo_new.png' },
  { id: 'momo', label: 'Ví MoMo', logo: '/momo_logo_new.png' },
  { id: 'banking', label: 'Chuyển Khoản Ngân Hàng', logo: 'https://payos.vn/docs/img/logo.svg' },
];

const CheckoutSim: React.FC = () => {
  const { bookingId } = useParams();
  const navigate = useNavigate();
  const { bookings, simulatePayOSPayment } = useData();
  
  const [payMethod, setPayMethod] = useState('card');
  const [processing, setProcessing] = useState(false);
  const [success, setSuccess] = useState(false);
  const [fallbackMode, setFallbackMode] = useState(false);
  const [payError, setPayError] = useState('');
  const [qrUrl, setQrUrl] = useState('');
  const [showQrModal, setShowQrModal] = useState(false);

  const booking = bookings.find((b) => b.id === bookingId);

  if (!booking) {
    return (
      <div className="min-h-screen bg-brand-black flex items-center justify-center p-8">
        <div className="text-center animate-fade-up">
          <p className="text-white/60 text-lg mb-4">Đang tải thông tin đơn hàng...</p>
          <button
            onClick={() => navigate('/customer/bookings')}
            className="flex items-center gap-2 px-6 py-3 bg-brand-surface border border-brand-border text-white hover:border-brand-red transition-all duration-200 mx-auto"
          >
            <ArrowLeft size={16} /> Quay lại
          </button>
        </div>
      </div>
    );
  }

  const total = booking.price;
  const fee = Math.round(total * 0.1);
  const basePrice = total - fee;

  const handlePay = async () => {
    setProcessing(true);
    setPayError('');
    
    // Check if it's a mock booking ID (e.g., does not match GUID pattern or contains mock)
    const isGuid = /^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$/i.test(booking.id);
    if (!isGuid || booking.id.includes('mock')) {
      const mockCheckoutUrl = `https://pay.payos.vn/checkout/mock-${booking.id}`;
      setQrUrl(mockCheckoutUrl);
      setShowQrModal(true);
      setProcessing(false);
      return;
    }

    try {
      const res = await api.post('/payments/create-link', {
        bookingId: booking.id,
      });
      if (res.data?.success && res.data.data?.checkoutUrl) {
        // Show QR Code modal instead of redirecting away
        setQrUrl(res.data.data.checkoutUrl);
        setShowQrModal(true);
      } else {
        setFallbackMode(true);
      }
    } catch (err: any) {
      console.error('Failed to create payment link:', err);
      setPayError(err.response?.data?.message || 'Không thể tạo link thanh toán PayOS.');
      setFallbackMode(true);
    } finally {
      setProcessing(false);
    }
  };

  if (success) {
    return (
      <div className="min-h-screen bg-brand-black flex items-center justify-center p-8">
        <div className="w-full max-w-md text-center animate-fade-up">
          <div className="w-20 h-20 bg-brand-red flex items-center justify-center mx-auto mb-8 rounded-xl">
            <CheckCircle size={40} className="text-white" />
          </div>
          <div className="w-12 h-0.5 bg-brand-red mx-auto mb-6" />
          <h2 className="font-montserrat font-black text-5xl text-white uppercase tracking-wider mb-4">
            Thanh Toán<br />Thành Công!
          </h2>
          <p className="text-white/50 text-base mb-4">
            Tiền của bạn đã được giữ trong hệ thống escrow an toàn. 
            HLV sẽ nhận được tiền sau khi hoàn thành buổi tập.
          </p>
          <p className="text-white/30 text-sm mb-10">Mã giao dịch: <span className="font-mono text-white">{bookingId}</span></p>

          <div className="bg-brand-surface border border-brand-border p-6 mb-8 text-left">
            <div className="flex items-center gap-3 mb-4">
              <Shield size={16} className="text-brand-red" />
              <p className="text-white font-semibold text-sm">Escrow Protection Active</p>
            </div>
            <p className="text-white/40 text-sm leading-relaxed">
              ₫{total.toLocaleString('vi-VN')} đang được bảo vệ bởi LockedIn. 
              Tiền sẽ chỉ chuyển cho HLV sau khi bạn xác nhận hoàn thành buổi tập.
            </p>
          </div>

          <button
            onClick={() => navigate('/customer/bookings')}
            className="btn-primary w-full justify-center py-4 cursor-pointer"
          >
            Xem Lịch Đặt Của Tôi
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-brand-black mobile-content-pad">
      <div className="border-b border-brand-border bg-brand-dark">
        <div className="section-container py-8">
          <button
            onClick={() => navigate(-1)}
            className="flex items-center gap-2 text-white/40 hover:text-white text-xs uppercase tracking-widest mb-6 cursor-pointer transition-colors duration-200"
          >
            <ArrowLeft size={14} />
            Quay Lại
          </button>
          <p className="section-label mb-2">Thanh Toán Escrow</p>
          <h1 className="page-title">Xác Nhận Đặt Lịch</h1>
        </div>
      </div>

      <div className="section-container py-8">
        <div className="grid lg:grid-cols-5 gap-8">
          {/* Left: Form */}
          <div className="lg:col-span-3">
            {/* Payment method */}
            <h3 className="font-montserrat font-bold text-xl text-white uppercase tracking-wider mb-5">Phương Thức Thanh Toán</h3>
            <div className="flex flex-col gap-3 mb-8">
              {PAYMENT_METHODS.map((m) => (
                <button
                  key={m.id}
                  onClick={() => setPayMethod(m.id)}
                  className={`flex items-center gap-4 p-4 border-2 cursor-pointer transition-all duration-200 text-left ${
                    payMethod === m.id ? 'border-brand-red bg-brand-red/10' : 'border-brand-border bg-brand-surface hover:border-white/20'
                  }`}
                >
                  <div className="w-12 h-12 bg-white flex items-center justify-center border border-brand-border flex-shrink-0">
                    <img 
                      src={m.logo} 
                      alt={m.label} 
                      className={`max-h-full max-w-full object-contain ${
                        m.id === 'banking' ? 'p-0.5 scale-[1.35]' : 'p-1'
                      }`} 
                    />
                  </div>
                  <span className={`font-semibold text-sm ${payMethod === m.id ? 'text-white' : 'text-white/60'}`}>{m.label}</span>
                  <div className={`ml-auto w-4 h-4 rounded-full border-2 transition-colors ${payMethod === m.id ? 'border-brand-red bg-brand-red' : 'border-brand-muted'}`} />
                </button>
              ))}
            </div>

            {/* Card form */}
            {payMethod === 'card' && (
              <div className="flex flex-col gap-4 animate-fade-in">
                <div>
                  <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Số Thẻ</label>
                  <input type="text" placeholder="1234 5678 9012 3456" className="input-dark" maxLength={19} />
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div>
                    <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Ngày Hết Hạn</label>
                    <input type="text" placeholder="MM/YY" className="input-dark" />
                  </div>
                  <div>
                    <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">CVV</label>
                    <input type="text" placeholder="•••" className="input-dark" maxLength={4} />
                  </div>
                </div>
                <div>
                  <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Tên Chủ Thẻ</label>
                  <input type="text" placeholder="NGUYEN VAN A" className="input-dark uppercase" />
                </div>
              </div>
            )}

            {/* Escrow explanation */}
            <div className="mt-8 border border-brand-border bg-brand-surface p-5">
              <div className="flex items-start gap-3">
                <Shield size={16} className="text-brand-red mt-0.5 flex-shrink-0" />
                <div>
                  <p className="text-white font-semibold text-sm mb-2">Thanh Toán Qua Escrow — Hoàn Toàn An Toàn</p>
                  <p className="text-white/40 text-xs leading-relaxed">
                    Tiền của bạn sẽ được giữ bởi LockedIn và chỉ được chuyển cho HLV 
                    sau khi mỗi buổi tập được xác nhận hoàn thành. Nếu có tranh chấp, 
                    Admin sẽ giải quyết công bằng.
                  </p>
                </div>
              </div>
            </div>
          </div>

          {/* Right: Order Summary */}
          <div className="lg:col-span-2">
            <div className="bg-brand-surface border border-brand-border sticky top-20">
              <div className="h-0.5 w-full bg-brand-red" />
              <div className="p-6">
                <h3 className="font-montserrat font-bold text-xl text-white uppercase tracking-wider mb-6">Tóm Tắt Đơn</h3>

                {/* PT Info */}
                <div className="flex items-center gap-3 p-4 bg-brand-dark border border-brand-border mb-6">
                  <div className="w-12 h-12 bg-brand-surface border border-brand-red flex items-center justify-center flex-shrink-0">
                    <span className="font-display text-base text-white">
                      {booking.ptName.split(' ').map((w) => w[0]).join('').slice(0, 2)}
                    </span>
                  </div>
                  <div>
                    <p className="text-white font-semibold text-sm">{booking.ptName}</p>
                    <p className="text-white/40 text-xs">{booking.packageName} · {booking.sessionsCount} buổi</p>
                  </div>
                </div>

                {/* Price breakdown */}
                <div className="flex flex-col gap-3 mb-6">
                  {[
                    { label: `Giá Gói (${booking.sessionsCount} buổi)`, value: basePrice.toLocaleString('vi-VN') + 'đ' },
                    { label: 'Phí Nền Tảng (10%)', value: fee.toLocaleString('vi-VN') + 'đ' },
                  ].map((item, i) => (
                    <div key={i} className="flex justify-between text-sm">
                      <span className="text-white/40">{item.label}</span>
                      <span className="text-white">{item.value}</span>
                    </div>
                  ))}
                  <div className="border-t border-brand-border pt-3 flex justify-between">
                    <span className="text-white font-semibold">Tổng Cộng</span>
                    <span className="text-white font-bold text-lg">{total.toLocaleString('vi-VN')}đ</span>
                  </div>
                </div>

                {/* Pay button */}
                <button
                  onClick={handlePay}
                  disabled={processing}
                  className={`w-full py-4 text-sm font-semibold uppercase tracking-widest flex items-center justify-center gap-3 transition-all duration-200 ${
                    processing ? 'bg-brand-muted text-white/50 cursor-not-allowed' : 'bg-brand-red text-white hover:bg-brand-red-dark cursor-pointer'
                  }`}
                >
                  {processing ? (
                    <>
                      <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                        <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                        <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                      </svg>
                      Đang Xử Lý...
                    </>
                  ) : (
                    <>
                      <Lock size={16} />
                      Thanh Toán An Toàn
                    </>
                  )}
                </button>

                <div className="flex items-center justify-center gap-2 mt-4 text-white/20 text-xs">
                  <Lock size={10} />
                  <span>Bảo mật bởi SSL 256-bit</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      {/* Premium Fallback Simulator Modal */}
      {fallbackMode && (
        <div className="fixed inset-0 bg-black/80 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-brand-surface border border-brand-red w-full max-w-md p-6 relative animate-fade-up">
            <div className="w-12 h-12 bg-brand-red/10 border border-brand-red flex items-center justify-center mb-4">
              <Shield size={24} className="text-brand-red" />
            </div>
            <h3 className="font-montserrat font-bold text-xl text-white uppercase tracking-wider mb-2">Cổng Thanh Toán PayOS</h3>
            <p className="text-white/60 text-xs leading-relaxed mb-4">
              {payError || 'Hệ thống không thể tạo liên kết thanh toán PayOS (do thiếu API Keys trong file appsettings.json của Backend).'}
            </p>
            <div className="bg-brand-dark border border-brand-border p-4 mb-4">
              <p className="text-brand-red font-semibold text-xs uppercase tracking-widest mb-2">Chế độ giả lập cục bộ</p>
              <p className="text-white/40 text-xs leading-relaxed mb-3">
                Bạn có thể tiếp tục bằng cách chạy script PowerShell bên dưới để đổi trạng thái đơn hàng sang **Đã thanh toán (Paid)** trong database, hoặc bấm nút giả lập nhanh.
              </p>
              <div className="bg-black/50 p-2 font-mono text-[10px] text-white/80 select-all break-all border border-brand-border mb-3">
                powershell -ExecutionPolicy Bypass -File pay_booking.ps1 -bookingId {booking.id}
              </div>
            </div>
            <div className="flex flex-col gap-2">
              <button
                onClick={async () => {
                  setProcessing(true);
                  await simulatePayOSPayment(booking.id);
                  setProcessing(false);
                  setSuccess(true);
                  setFallbackMode(false);
                }}
                className="btn-primary w-full justify-center py-3 text-xs cursor-pointer"
              >
                Xác Nhận Giả Lập Thành Công (Nhanh)
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Premium PayOS QR Code Modal */}
      {showQrModal && (
        <div className="fixed inset-0 bg-black/80 backdrop-blur-sm z-50 flex items-center justify-center p-4">
          <div className="bg-brand-surface border border-brand-red w-full max-w-md p-6 relative animate-fade-up text-center">
            <button 
              onClick={() => setShowQrModal(false)}
              className="absolute top-4 right-4 text-white/40 hover:text-white cursor-pointer"
            >
              <X size={20} />
            </button>
            
            <div className="w-12 h-12 bg-brand-red/10 border border-brand-red flex items-center justify-center mx-auto mb-4">
              <CreditCard size={24} className="text-brand-red" />
            </div>
            
            <h3 className="font-montserrat font-bold text-xl text-white uppercase tracking-wider mb-2">Thanh Toán Quét Mã QR</h3>
            <p className="text-white/60 text-xs mb-6">
              Sử dụng ứng dụng Ngân hàng hoặc Ví điện tử quét mã QR bên dưới để thực hiện thanh toán an toàn qua PayOS.
            </p>
            
            {/* QR Code Container */}
            <div className="bg-white p-4 inline-block rounded-lg mb-6 border-4 border-brand-red shadow-[0_0_20px_rgba(230,0,0,0.3)]">
              <img 
                src={`https://api.qrserver.com/v1/create-qr-code/?size=200x200&data=${encodeURIComponent(qrUrl)}`} 
                alt="PayOS QR Code" 
                className="w-48 h-48 mx-auto"
              />
            </div>
            
            <div className="flex flex-col gap-2">
              <a
                href={qrUrl}
                target="_blank"
                rel="noopener noreferrer"
                className="btn-primary w-full justify-center py-3 text-xs cursor-pointer flex items-center justify-center gap-2"
              >
                Mở Trang Thanh Toán PayOS <ExternalLink size={12} />
              </a>
              <button
                onClick={async () => {
                  setProcessing(true);
                  await simulatePayOSPayment(booking.id);
                  setProcessing(false);
                  setSuccess(true);
                  setShowQrModal(false);
                }}
                className="w-full py-3 border border-brand-border text-white/50 text-xs font-semibold uppercase tracking-widest hover:text-white transition-all cursor-pointer text-center"
              >
                Tôi Đã Thanh Toán Thành Công
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default CheckoutSim;
