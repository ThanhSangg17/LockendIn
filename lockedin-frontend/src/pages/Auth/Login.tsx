// src/pages/Auth/Login.tsx
import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Eye, EyeOff, Zap, ArrowRight, Loader2 } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { GoogleLogin } from '@react-oauth/google';
import api from '../../services/api';
import logoImg from '../../assets/logo.png';

const Login: React.FC = () => {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [showPass, setShowPass] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const { login, googleLogin } = useAuth();
  const navigate = useNavigate();

  // Forgot / Reset Password state
  const [showForgotModal, setShowForgotModal] = useState(false);
  const [forgotTab, setForgotTab] = useState<'request' | 'reset'>('request');
  const [forgotEmail, setForgotEmail] = useState('');
  const [resetToken, setResetToken] = useState('');
  const [newPassword, setNewPassword] = useState('');
  const [forgotMsg, setForgotMsg] = useState('');
  const [forgotError, setForgotError] = useState('');
  const [forgotLoading, setForgotLoading] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');
    setLoading(true);
    try {
      const result = await login(email, password);
      if (result.success) {
        if (result.role === 'customer') navigate('/');
        else if (result.role === 'pt') navigate('/pt/bookings');
        else if (result.role === 'admin') navigate('/admin/dashboard');
        else navigate('/');
      } else {
        setError('Email hoặc mật khẩu không chính xác');
      }
    } catch {
      setError('Đã có lỗi xảy ra, vui lòng thử lại');
    } finally {
      setLoading(false);
    }
  };

  const handleForgotPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setForgotMsg('');
    setForgotError('');
    setForgotLoading(true);
    try {
      const res = await api.post('/auth/forgot-password', {
        email: forgotEmail
      });
      if (res.data?.success) {
        setForgotMsg(res.data.message || 'Yêu cầu đặt lại mật khẩu đã được gửi!');
        // Automatically switch to reset tab after a short delay
        setTimeout(() => {
          setForgotTab('reset');
          setForgotMsg('');
        }, 2000);
      } else {
        setForgotError(res.data?.message || 'Không thể gửi yêu cầu đặt lại mật khẩu.');
      }
    } catch (err: any) {
      setForgotError(err.response?.data?.message || err.message || 'Lỗi kết nối máy chủ.');
    } finally {
      setForgotLoading(false);
    }
  };

  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setForgotMsg('');
    setForgotError('');
    
    const passRegex = /^(?=.*[a-zA-Z])(?=.*\d).+$/;
    if (newPassword.length < 8 || !passRegex.test(newPassword)) {
      setForgotError('Mật khẩu mới phải dài tối thiểu 8 ký tự và chứa ít nhất một chữ cái và một chữ số.');
      return;
    }

    setForgotLoading(true);
    try {
      const res = await api.post('/auth/reset-password', {
        email: forgotEmail,
        token: resetToken,
        newPassword: newPassword
      });
      if (res.data?.success) {
        setForgotMsg('Đặt lại mật khẩu thành công! Hãy đóng hộp thoại này và đăng nhập.');
        setNewPassword('');
        setResetToken('');
      } else {
        setForgotError(res.data?.message || 'Đặt lại mật khẩu thất bại.');
      }
    } catch (err: any) {
      setForgotError(err.response?.data?.message || err.message || 'Lỗi kết nối máy chủ.');
    } finally {
      setForgotLoading(false);
    }
  };

  return (
    <div 
      className="min-h-screen flex relative"
      style={{
        backgroundImage: 'url("https://images.unsplash.com/photo-1571019614242-c5c5dee9f50b?q=80&w=2000&auto=format&fit=crop")',
        backgroundSize: 'cover',
        backgroundPosition: 'center',
      }}
    >
      {/* Global Overlay */}
      <div className="absolute inset-0 bg-black/40" />

      {/* Left side text container */}
      <div className="relative z-10 hidden lg:flex flex-1 flex-col justify-center p-16 xl:p-24">
        <div>
          <Link to="/" className="inline-block mb-12 hover:opacity-80 transition-opacity">
            <img src={logoImg} alt="LockedIn Logo" className="h-12 w-auto object-contain drop-shadow-lg" />
          </Link>
          <div className="w-12 h-1 bg-brand-red mb-6 shadow-[0_0_10px_rgba(255,0,0,0.5)]" />
          <h2 className="font-montserrat font-black text-6xl text-white uppercase leading-none tracking-tight mb-6 drop-shadow-xl">
            CHINH PHỤC<br />
            <span className="text-brand-red drop-shadow-[0_0_15px_rgba(255,0,0,0.4)]">GIỚI HẠN</span><br />
            CỦA BẠN
          </h2>
          <p className="text-white/90 text-lg leading-relaxed max-w-sm drop-shadow-md">
            Đăng nhập để tiếp tục hành trình chinh phục vóc dáng cùng các huấn luyện viên hàng đầu Việt Nam.
          </p>
        </div>
      </div>

      {/* Right side form container */}
      <div className="relative z-10 w-full lg:w-1/2 flex items-center justify-center p-4 lg:p-12">
        <div className="w-full max-w-md bg-white/5 backdrop-blur-xl border border-white/20 rounded-3xl p-8 lg:p-10 shadow-2xl">
          <Link to="/" className="flex justify-center mb-10 lg:hidden hover:opacity-80 transition-opacity">
            <img src={logoImg} alt="LockedIn Logo" className="h-10 w-auto object-contain" />
          </Link>

          <div className="mb-10 text-center">
            <p className="section-label mb-2 text-white/80">Chào Mừng Trở Lại</p>
            <h1 className="font-montserrat font-black text-3xl text-white uppercase tracking-wider">Đăng Nhập</h1>
          </div>

          {error && (
            <div className="mb-6 border border-brand-red bg-brand-red/10 px-4 py-3 text-brand-red text-sm">
              {error}
            </div>
          )}

          <form onSubmit={handleSubmit} className="flex flex-col gap-5">
            <div>
              <label className="block text-white/70 text-xs uppercase tracking-widest mb-2">Email</label>
              <input
                type="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                placeholder="your@email.com"
                required
                className="input-dark"
              />
            </div>

            <div>
              <label className="block text-white/70 text-xs uppercase tracking-widest mb-2">Mật Khẩu</label>
              <div className="relative">
                <input
                  type={showPass ? 'text' : 'password'}
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                  placeholder="••••••••"
                  required
                  className="input-dark pr-12"
                />
                <button
                  type="button"
                  onClick={() => setShowPass(!showPass)}
                  className="absolute right-4 top-1/2 -translate-y-1/2 text-white/30 hover:text-white transition-colors cursor-pointer"
                >
                  {showPass ? <EyeOff size={16} /> : <Eye size={16} />}
                </button>
              </div>
            </div>

            <div className="flex justify-end">
              <button
                type="button"
                onClick={() => {
                  setShowForgotModal(true);
                  setForgotTab('request');
                  setForgotEmail('');
                  setResetToken('');
                  setNewPassword('');
                  setForgotMsg('');
                  setForgotError('');
                }}
                className="text-xs text-white/30 hover:text-brand-red transition-colors uppercase tracking-widest cursor-pointer bg-transparent border-0 outline-none"
              >
                Quên Mật Khẩu?
              </button>
            </div>

            <button
              type="submit"
              disabled={loading}
              className={`btn-primary w-full justify-center py-4 text-sm mt-2 ${loading ? 'opacity-50 cursor-not-allowed' : ''}`}
            >
              {loading ? (
                <svg className="animate-spin h-4 w-4" fill="none" viewBox="0 0 24 24">
                  <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="4" />
                  <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4z" />
                </svg>
              ) : (
                <>Đăng Nhập <ArrowRight size={16} /></>
              )}
            </button>
            
            <div className="relative my-6">
              <div className="absolute inset-0 flex items-center">
                <div className="w-full border-t border-white/10"></div>
              </div>
              <div className="relative flex justify-center text-xs">
                <span className="bg-[#0f0f13] px-4 text-white/40 tracking-wider">HOẶC ĐĂNG NHẬP BẰNG</span>
              </div>
            </div>

            <div className="flex justify-center w-full">
              <GoogleLogin
                onSuccess={async (credentialResponse) => {
                  if (credentialResponse.credential) {
                    setLoading(true);
                    setError('');
                    const res = await googleLogin(credentialResponse.credential);
                    if (res.success) {
                      if (res.role === 'pt') navigate('/pt/dashboard');
                      else if (res.role === 'admin') navigate('/admin/dashboard');
                      else navigate('/customer/dashboard');
                    } else {
                      setError(res.message || 'Google login failed');
                      setLoading(false);
                    }
                  }
                }}
                onError={() => {
                  setError('Login Failed');
                }}
                theme="filled_black"
                shape="rectangular"
                text="continue_with"
                size="large"
                width="300"
              />
            </div>
          </form>

          <p className="text-center text-white/30 text-sm mt-8">
            Chưa có tài khoản?{' '}
            <Link to="/register" className="text-white hover:text-brand-red transition-colors font-semibold">
              Đăng ký ngay
            </Link>
          </p>


        </div>
      </div>

      {/* Forgot & Reset Password Modal */}
      {showForgotModal && (
        <div className="fixed inset-0 bg-black/80 flex items-center justify-center p-4 z-50 animate-fade-in">
          <div className="bg-brand-dark border-2 border-brand-border p-6 w-full max-w-md relative">
            <button
              type="button"
              onClick={() => setShowForgotModal(false)}
              className="absolute top-4 right-4 text-white/40 hover:text-white cursor-pointer transition-colors text-lg"
            >
              &times;
            </button>
            
            <div className="flex items-center gap-2 mb-4">
              <Zap className="text-brand-red" size={20} />
              <h3 className="font-montserrat font-bold text-lg text-white uppercase tracking-wider">Khôi Phục Mật Khẩu</h3>
            </div>

            {/* Modal Tabs */}
            <div className="flex border-b border-brand-border mb-5">
              <button
                type="button"
                onClick={() => {
                  setForgotTab('request');
                  setForgotMsg('');
                  setForgotError('');
                }}
                className={`flex-1 py-2 text-center text-[10px] font-bold uppercase tracking-widest cursor-pointer border-b-2 transition-all ${
                  forgotTab === 'request' ? 'text-white border-brand-red' : 'text-white/40 border-transparent hover:text-white'
                }`}
              >
                1. Gửi Mã Reset
              </button>
              <button
                type="button"
                onClick={() => {
                  setForgotTab('reset');
                  setForgotMsg('');
                  setForgotError('');
                }}
                className={`flex-1 py-2 text-center text-[10px] font-bold uppercase tracking-widest cursor-pointer border-b-2 transition-all ${
                  forgotTab === 'reset' ? 'text-white border-brand-red' : 'text-white/40 border-transparent hover:text-white'
                }`}
              >
                2. Đặt Lại Mật Khẩu
              </button>
            </div>

            {forgotMsg && (
              <div className="mb-4 border border-white bg-white/5 px-3 py-2 text-white text-xs rounded-xl">
                {forgotMsg}
              </div>
            )}

            {forgotError && (
              <div className="mb-4 border border-brand-red bg-brand-red/10 px-3 py-2 text-brand-red text-xs">
                {forgotError}
              </div>
            )}

            {forgotTab === 'request' ? (
              <form onSubmit={handleForgotPassword} className="flex flex-col gap-4">
                <div>
                  <label className="block text-[10px] text-white/40 uppercase tracking-widest mb-1.5 font-bold">Địa chỉ Email</label>
                  <input
                    type="email"
                    value={forgotEmail}
                    onChange={(e) => setForgotEmail(e.target.value)}
                    placeholder="your@email.com"
                    required
                    className="input-dark"
                    disabled={forgotLoading}
                  />
                </div>

                <div className="flex justify-end gap-3 mt-2">
                  <button
                    type="button"
                    onClick={() => setShowForgotModal(false)}
                    className="px-4 py-2 border border-brand-border text-white/60 hover:text-white text-xs font-semibold uppercase tracking-wider cursor-pointer"
                  >
                    Đóng
                  </button>
                  <button
                    type="submit"
                    disabled={forgotLoading}
                    className="btn-primary py-2 px-4 text-xs tracking-wider justify-center min-w-[120px]"
                  >
                    {forgotLoading ? (
                      <Loader2 className="animate-spin text-white" size={12} />
                    ) : (
                      'GỬI YÊU CẦU'
                    )}
                  </button>
                </div>
              </form>
            ) : (
              <form onSubmit={handleResetPassword} className="flex flex-col gap-4">
                <div>
                  <label className="block text-[10px] text-white/40 uppercase tracking-widest mb-1.5 font-bold">Địa chỉ Email</label>
                  <input
                    type="email"
                    value={forgotEmail}
                    onChange={(e) => setForgotEmail(e.target.value)}
                    placeholder="your@email.com"
                    required
                    className="input-dark"
                    disabled={forgotLoading}
                  />
                </div>

                <div>
                  <label className="block text-[10px] text-white/40 uppercase tracking-widest mb-1.5 font-bold">Mã xác thực (Token)</label>
                  <input
                    type="text"
                    value={resetToken}
                    onChange={(e) => setResetToken(e.target.value)}
                    placeholder="Nhập mã token từ email"
                    required
                    className="input-dark font-mono"
                    disabled={forgotLoading}
                  />
                </div>

                <div>
                  <label className="block text-[10px] text-white/40 uppercase tracking-widest mb-1.5 font-bold">Mật khẩu mới</label>
                  <input
                    type="password"
                    value={newPassword}
                    onChange={(e) => setNewPassword(e.target.value)}
                    placeholder="Tối thiểu 8 ký tự (chứa chữ và số)"
                    required
                    className="input-dark"
                    disabled={forgotLoading}
                  />
                </div>

                <div className="flex justify-end gap-3 mt-2">
                  <button
                    type="button"
                    onClick={() => setShowForgotModal(false)}
                    className="px-4 py-2 border border-brand-border text-white/60 hover:text-white text-xs font-semibold uppercase tracking-wider cursor-pointer"
                  >
                    Đóng
                  </button>
                  <button
                    type="submit"
                    disabled={forgotLoading}
                    className="btn-primary py-2 px-4 text-xs tracking-wider justify-center min-w-[120px]"
                  >
                    {forgotLoading ? (
                      <Loader2 className="animate-spin text-white" size={12} />
                    ) : (
                      'ĐẶT LẠI'
                    )}
                  </button>
                </div>
              </form>
            )}
          </div>
        </div>
      )}
    </div>
  );
};

export default Login;
