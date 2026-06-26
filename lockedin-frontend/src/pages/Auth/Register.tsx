// src/pages/Auth/Register.tsx
import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';
import { Eye, EyeOff, ArrowRight, User, Dumbbell } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';
import { GoogleLogin } from '@react-oauth/google';
import logoImg from '../../assets/logo.png';

const Register: React.FC = () => {
  const [role, setRole] = useState<'customer' | 'pt'>('customer');
  const [fullName, setFullName] = useState('');
  const [email, setEmail] = useState('');
  const [phone, setPhone] = useState('');
  const [password, setPassword] = useState('');
  const [showPass, setShowPass] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState('');
  const { register, googleLogin } = useAuth();
  const navigate = useNavigate();

  const [showTerms, setShowTerms] = useState(false);
  const [showPrivacy, setShowPrivacy] = useState(false);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError('');

    // Frontend validations to match backend regex constraints
    const phoneRegex = /^0\d{9}$/;
    if (!phoneRegex.test(phone)) {
      setError('Số điện thoại phải gồm đúng 10 chữ số và bắt đầu bằng số 0.');
      return;
    }

    const passRegex = /^(?=.*[a-zA-Z])(?=.*\d).+$/;
    if (password.length < 8 || !passRegex.test(password)) {
      setError('Mật khẩu phải dài tối thiểu 8 ký tự và chứa ít nhất một chữ cái và một chữ số.');
      return;
    }

    setLoading(true);
    try {
      await register(fullName, email, role, phone, password);
      navigate(role === 'pt' ? '/pt/packages' : '/onboarding');
    } catch (err: any) {
      console.error('Registration failed:', err);
      let errMsg = 'Đăng ký thất bại. Vui lòng kiểm tra lại thông tin.';
      if (err.response?.data?.errors) {
        const errors = err.response.data.errors;
        const msgList: string[] = [];
        Object.keys(errors).forEach((key) => {
          if (Array.isArray(errors[key])) {
            msgList.push(...errors[key]);
          }
        });
        errMsg = msgList.join(' | ');
      } else if (err.response?.data?.message) {
        errMsg = err.response.data.message;
      } else if (err.message) {
        errMsg = err.message;
      }
      setError(errMsg);
    } finally {
      setLoading(false);
    }
  };

  return (
    <div 
      className="min-h-screen flex relative"
      style={{
        backgroundImage: 'url("https://images.unsplash.com/photo-1517836357463-d25dfeac3438?q=80&w=2000&auto=format&fit=crop")',
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
            BẮT ĐẦU<br />
            <span className="text-brand-red drop-shadow-[0_0_15px_rgba(255,0,0,0.4)]">HÀNH TRÌNH</span><br />
            CỦA BẠN
          </h2>
          <p className="text-white/90 text-lg leading-relaxed max-w-sm drop-shadow-md">
            Tham gia cộng đồng LockedIn ngay hôm nay để tìm kiếm người đồng hành hoàn hảo cho mục tiêu của bạn.
          </p>
        </div>
      </div>

      {/* Right side form container */}
      <div className="relative z-10 w-full lg:w-1/2 flex items-center justify-center p-4 lg:p-8">
        <div className="w-full max-w-md bg-white/5 backdrop-blur-xl border border-white/20 rounded-3xl p-6 lg:p-8 shadow-2xl">
          <Link to="/" className="flex justify-center mb-6 lg:hidden hover:opacity-80 transition-opacity">
            <img src={logoImg} alt="LockedIn Logo" className="h-10 w-auto object-contain" />
          </Link>

          <div className="mb-6 text-center">
            <p className="section-label mb-1 text-white/80">Bắt Đầu Ngay Hôm Nay</p>
            <h1 className="font-montserrat font-black text-2xl text-white uppercase tracking-wider">Tạo Tài Khoản</h1>
          </div>

        {/* Role Selector */}
        <div className="grid grid-cols-2 gap-2 mb-6">
          {([
            { id: 'customer', label: 'Người Tập Luyện', icon: User, desc: 'Tìm HLV & theo dõi' },
            { id: 'pt', label: 'Huấn Luyện Viên', icon: Dumbbell, desc: 'Nhận học viên' },
          ] as const).map((r) => {
            const Icon = r.icon;
            return (
              <button
                key={r.id}
                type="button"
                onClick={() => setRole(r.id)}
                className={`flex flex-col items-start p-3 border-2 cursor-pointer transition-all duration-200 text-left ${
                  role === r.id
                    ? 'border-brand-red bg-brand-red/10'
                    : 'border-brand-border hover:border-white/20 bg-brand-surface'
                }`}
              >
                <div className={`w-6 h-6 flex items-center justify-center mb-2 ${role === r.id ? 'bg-brand-red' : 'bg-brand-dark'}`}>
                  <Icon size={14} className="text-white" />
                </div>
                <p className={`font-semibold text-sm mb-0.5 ${role === r.id ? 'text-white' : 'text-white/60'}`}>{r.label}</p>
                <p className="text-white/30 text-[10px] leading-tight">{r.desc}</p>
              </button>
            );
          })}
        </div>

        {error && (
          <div className="mb-4 border border-brand-red bg-brand-red/10 px-3 py-2 text-brand-red text-sm">
            {error}
          </div>
        )}

        <form onSubmit={handleSubmit} className="flex flex-col gap-3">
          <div className="grid grid-cols-2 gap-3">
            <div>
              <label className="block text-white/70 text-xs uppercase tracking-widest mb-1.5">Họ và Tên</label>
              <input
                type="text"
                value={fullName}
                onChange={(e) => setFullName(e.target.value)}
                placeholder="Nguyễn Văn A"
                required
                className="input-dark py-2"
              />
            </div>
            <div>
              <label className="block text-white/70 text-xs uppercase tracking-widest mb-1.5">SĐT</label>
              <input
                type="tel"
                value={phone}
                onChange={(e) => setPhone(e.target.value)}
                placeholder="0987..."
                required
                className="input-dark py-2"
              />
            </div>
          </div>

          <div>
            <label className="block text-white/70 text-xs uppercase tracking-widest mb-1.5">Email</label>
            <input
              type="email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="your@email.com"
              required
              className="input-dark py-2"
            />
          </div>

          <div>
            <label className="block text-white/70 text-xs uppercase tracking-widest mb-1.5">Mật Khẩu</label>
            <div className="relative">
              <input
                type={showPass ? 'text' : 'password'}
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                placeholder="Tối thiểu 8 ký tự"
                required
                minLength={8}
                className="input-dark pr-12 py-2"
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

          <p className="text-white/20 text-xs leading-relaxed">
            Bằng cách đăng ký, bạn đồng ý với{' '}
            <button 
              type="button" 
              onClick={() => setShowTerms(true)}
              className="text-white/50 hover:text-brand-red transition-colors underline cursor-pointer bg-transparent border-0 p-0 outline-none text-xs"
            >
              Điều Khoản Dịch Vụ
            </button>
            {' '}và{' '}
            <button 
              type="button" 
              onClick={() => setShowPrivacy(true)}
              className="text-white/50 hover:text-brand-red transition-colors underline cursor-pointer bg-transparent border-0 p-0 outline-none text-xs"
            >
              Chính Sách Bảo Mật
            </button>.
          </p>

          <div className="flex flex-col gap-2 pt-1">
            <button
              type="submit"
              disabled={loading}
              className="btn-primary py-2.5 px-4 text-xs tracking-wider justify-center w-full"
            >
              {loading ? (
                <Loader2 className="animate-spin text-white" size={16} />
              ) : (
                'TẠO TÀI KHOẢN'
              )}
            </button>
          </div>

          {role === 'customer' && (
            <>
              <div className="relative my-2">
                <div className="absolute inset-0 flex items-center">
                  <div className="w-full border-t border-white/10"></div>
                </div>
                <div className="relative flex justify-center text-[10px]">
                  <span className="bg-[#0f0f13] px-3 text-white/40 tracking-wider">HOẶC ĐĂNG KÝ BẰNG</span>
                </div>
              </div>

              <div className="flex justify-center w-full scale-90 origin-top">
                <GoogleLogin
                  onSuccess={async (credentialResponse) => {
                    if (credentialResponse.credential) {
                      setLoading(true);
                      setError('');
                      const res = await googleLogin(credentialResponse.credential, 'customer');
                      if (res.success) {
                        navigate('/customer/dashboard');
                      } else {
                        setError(res.message || 'Google registration failed');
                        setLoading(false);
                      }
                    }
                  }}
                  onError={() => {
                    setError('Registration Failed');
                  }}
                  theme="filled_black"
                  shape="rectangular"
                  text="signup_with"
                  size="large"
                  width="300"
                />
              </div>
            </>
          )}
        </form>

        <p className="text-center text-white/30 text-xs mt-6">
          Đã có tài khoản?{' '}
          <Link to="/login" className="text-white hover:text-brand-red transition-colors font-semibold">
            Đăng nhập
          </Link>
        </p>
        </div>
      </div>

      {/* Terms of Service Modal */}
      {showTerms && (
        <div className="fixed inset-0 bg-black/80 flex items-center justify-center p-4 z-50 animate-fade-in">
          <div className="bg-brand-dark border-2 border-brand-border p-6 w-full max-w-2xl max-h-[80vh] flex flex-col relative">
            <button
              type="button"
              onClick={() => setShowTerms(false)}
              className="absolute top-4 right-4 text-white/40 hover:text-white cursor-pointer transition-colors text-lg"
            >
              &times;
            </button>
            
            <div className="flex items-center gap-2 mb-4">
              <h3 className="font-montserrat font-bold text-lg text-white uppercase tracking-wider">Điều Khoản Dịch Vụ (LockedIn)</h3>
            </div>

            <div className="overflow-y-auto text-white/60 text-sm leading-relaxed pr-2 flex-1 flex flex-col gap-4">
              <p>Chào mừng bạn đến với <strong>LockedIn</strong>. Bằng cách đăng ký tài khoản và sử dụng dịch vụ của chúng tôi, bạn đồng ý tuân thủ các điều khoản sau:</p>
              
              <div>
                <h4 className="text-white font-semibold mb-1">1. Tài Khoản Người Dùng</h4>
                <p>Bạn phải cung cấp thông tin chính xác, đầy đủ và cập nhật khi tạo tài khoản. Bạn có trách nhiệm bảo mật thông tin mật khẩu và chịu trách nhiệm cho mọi hoạt động diễn ra dưới tài khoản của mình.</p>
              </div>

              <div>
                <h4 className="text-white font-semibold mb-1">2. Dịch Vụ Huấn Luyện & Giao Dịch Ký Quỹ (Escrow)</h4>
                <p>LockedIn cung cấp cổng kết nối giữa Học viên và Huấn luyện viên (PT). Tiền thanh toán khóa học sẽ được giữ trong hệ thống ký quỹ (Escrow) của LockedIn và chỉ giải phóng (thanh toán cho PT) khi các buổi học đã hoàn thành theo lộ trình cam kết.</p>
              </div>

              <div>
                <h4 className="text-white font-semibold mb-1">3. Chính Sách Huỷ Khóa Học & Hoàn Tiền</h4>
                <p>Học viên và PT được yêu cầu thỏa thuận lịch tập trước khi bắt đầu. Nếu xảy ra tranh chấp hoặc PT không thực hiện đúng nghĩa vụ dạy học, Học viên có quyền gửi yêu cầu "Khiếu Nại" kèm minh chứng để hệ thống xem xét hoàn trả tiền ký quỹ.</p>
              </div>

              <div>
                <h4 className="text-white font-semibold mb-1">4. Quy Tắc Ứng Xử</h4>
                <p>Mọi thành viên cam kết ứng xử văn minh lịch sự trên không gian phòng chat chung và trong các buổi tập thực tế. Nghiêm cấm mọi hành vi gian lận, xúc phạm danh dự hoặc chia sẻ nội dung độc hại trên nền tảng.</p>
              </div>

              <div>
                <h4 className="text-white font-semibold mb-1">5. Thay Đổi Điều Khoản</h4>
                <p>LockedIn có quyền thay đổi điều khoản dịch vụ này bất kỳ lúc nào. Chúng tôi sẽ thông báo cho bạn thông qua email hoặc giao diện ứng dụng trước khi áp dụng các thay đổi lớn.</p>
              </div>
            </div>

            <div className="flex justify-end mt-6 pt-4 border-t border-brand-border/60">
              <button
                type="button"
                onClick={() => setShowTerms(false)}
                className="btn-primary py-2 px-6 text-xs uppercase tracking-wider cursor-pointer"
              >
                ĐÃ ĐỌC & ĐỒNG Ý
              </button>
            </div>
          </div>
        </div>
      )}

      {/* Privacy Policy Modal */}
      {showPrivacy && (
        <div className="fixed inset-0 bg-black/80 flex items-center justify-center p-4 z-50 animate-fade-in">
          <div className="bg-brand-dark border-2 border-brand-border p-6 w-full max-w-2xl max-h-[80vh] flex flex-col relative">
            <button
              type="button"
              onClick={() => setShowPrivacy(false)}
              className="absolute top-4 right-4 text-white/40 hover:text-white cursor-pointer transition-colors text-lg"
            >
              &times;
            </button>
            
            <div className="flex items-center gap-2 mb-4">
              <h3 className="font-montserrat font-bold text-lg text-white uppercase tracking-wider">Chính Sách Bảo Mật (LockedIn)</h3>
            </div>

            <div className="overflow-y-auto text-white/60 text-sm leading-relaxed pr-2 flex-1 flex flex-col gap-4">
              <p>Chính Sách Bảo Mật này giải thích cách <strong>LockedIn</strong> thu thập, sử dụng và bảo vệ thông tin cá nhân của bạn khi tham gia nền tảng:</p>
              
              <div>
                <h4 className="text-white font-semibold mb-1">1. Thông Tin Thu Thập</h4>
                <p>Chúng tôi thu thập các thông tin bạn cung cấp trực tiếp như: Họ và tên, Email, Số điện thoại, Ảnh đại diện, và các minh chứng (CCCD, Chứng chỉ HLV đối với PT) nhằm mục đích xác thực tài khoản và đảm bảo an toàn cho học viên.</p>
              </div>

              <div>
                <h4 className="text-white font-semibold mb-1">2. Sử Dụng Thông Tin</h4>
                <p>Thông tin của bạn được sử dụng để: Cung cấp và duy trì dịch vụ LockedIn, kết nối Học viên với PT phù hợp, gửi thông báo xác nhận lịch học, xử lý các giao dịch thanh toán và hỗ trợ giải quyết tranh chấp khiếu nại.</p>
              </div>

              <div>
                <h4 className="text-white font-semibold mb-1">3. Chia Sẻ Thông Tin</h4>
                <p>Chúng tôi cam kết không bán hoặc cho thuê thông tin cá nhân của bạn cho bên thứ ba. Thông tin của bạn chỉ được chia sẻ giữa Học viên và PT được chỉ định trong cùng lớp học để phục vụ việc liên hệ tập luyện.</p>
              </div>

              <div>
                <h4 className="text-white font-semibold mb-1">4. Bảo Mật Dữ Liệu</h4>
                <p>LockedIn áp dụng các biện pháp bảo mật chuẩn hóa SSL/TLS để mã hóa dữ liệu truyền tải. Dữ liệu nhạy cảm như thông tin xác thực tài khoản và thanh toán được bảo vệ nghiêm ngặt trên hệ thống máy chủ cơ sở dữ liệu an toàn.</p>
              </div>

              <div>
                <h4 className="text-white font-semibold mb-1">5. Quyền Của Người Dùng</h4>
                <p>Bạn có quyền truy cập, chỉnh sửa hoặc yêu cầu xóa thông tin cá nhân của mình bất kỳ lúc nào trực tiếp trong phần quản lý Hồ sơ tài khoản hoặc liên hệ trực tiếp với bộ phận chăm sóc khách hàng của LockedIn.</p>
              </div>
            </div>

            <div className="flex justify-end mt-6 pt-4 border-t border-brand-border/60">
              <button
                type="button"
                onClick={() => setShowPrivacy(false)}
                className="btn-primary py-2 px-6 text-xs uppercase tracking-wider cursor-pointer"
              >
                ĐÃ ĐỌC & ĐỒNG Ý
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Register;
