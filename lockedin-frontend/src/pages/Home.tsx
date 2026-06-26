// src/pages/Home.tsx
import React, { useEffect, useRef, useState } from 'react';
import { Link } from 'react-router-dom';
import { Star, Shield, Zap, TrendingUp, Users, CheckCircle, ChevronRight, Play, Search, Dumbbell, Target, Clock, Calendar, Lock, LogOut, Heart, Activity, PlayCircle, Plus, Sparkles, MessageSquare, Flame, Quote, Droplets, MapPin, ExternalLink, ArrowRight, User, Home as HomeIcon } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import PTHome from './PT/PTHome';
import AdminHome from './AdminHome';
import logoImg from '../assets/logo.png';

const FEATURED_SHOWCASE = [
  {
    id: 1,
    name: 'HÙNG TITAN',
    specialty: 'Bodybuilding • Powerlifting • Nutrition',
    price: 500000,
    rating: 4.9,
    reviews: 120,
    experience: '10 năm',
    image: 'https://images.unsplash.com/photo-1534438327276-14e5300c3a48?q=80&w=1470&auto=format&fit=crop',
    avatar: 'https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?q=80&w=1480&auto=format&fit=crop',
    tags: ['Bodybuilding', 'Powerlifting', 'Nutrition'],
    bio: 'Chuyên gia huấn luyện thể hình với hơn 10 năm kinh nghiệm. Giúp bạn vượt qua giới hạn bản thân, đạt được mục tiêu tăng cơ giảm mỡ khoa học nhất.',
    comments: [
      { user: 'Trần Đức Khải', text: '"Báo cáo tiến độ chi tiết giúp tôi theo dõi quá trình cực kỳ rõ ràng. Không còn phải đoán mò về kết quả nữa!"' },
      { user: 'Nguyễn Minh Tuấn', text: '"Sau 3 tháng với HLV Hùng, tôi giảm được 12kg và tăng 8kg cơ. Lối sống của tôi thực sự thay đổi hoàn toàn."' },
      { user: 'Hoàng Thái Hưng', text: '"Kiến thức dinh dưỡng của HLV Hùng cực kỳ chuyên sâu. Lịch ăn không hề nhàm chán."' },
      { user: 'Lâm Tuấn Kiệt', text: '"Ban đầu tôi rất sợ tạ nặng, nhưng với phương pháp chuẩn, tôi đã có thể deadlift 150kg."' },
      { user: 'Vũ Đức Mạnh', text: '"Hệ thống theo dõi bài tập rất trực quan. Tôi luôn biết ngày mai mình cần tập gì."' }
    ]
  },
  {
    id: 2,
    name: 'NGỌC KIM',
    specialty: 'Yoga • Cardio • Flexibility',
    price: 350000,
    rating: 4.7,
    reviews: 85,
    experience: '6 năm',
    image: 'https://images.unsplash.com/photo-1599901860904-17e6ed7083a0?q=80&w=1470&auto=format&fit=crop',
    avatar: 'https://images.unsplash.com/photo-1438761681033-6461ffad8d80?q=80&w=1470&auto=format&fit=crop',
    tags: ['Yoga', 'Cardio', 'Flexibility'],
    bio: 'HLV Yoga tận tâm, tập trung vào phục hồi chấn thương và cải thiện tư thế. Mang lại sự dẻo dai và tinh thần thư thái sau mỗi buổi tập.',
    comments: [
      { user: 'Lê Thị Thu', text: '"HLV cực kỳ tận tâm, các bài tập phục hồi đã giúp tôi hết hẳn cơn đau thắt lưng kéo dài."' },
      { user: 'Phạm Văn A', text: '"Giáo án rất khoa học, từ tốn và dễ tập theo. Rất đáng tiền!"' },
      { user: 'Ngô Thanh Ngân', text: '"Nhờ những bài tập giãn cơ của PT, tôi không còn bị gù lưng và cải thiện tư thế."' },
      { user: 'Đào Thu Thủy', text: '"Cảm giác mỗi buổi tập như một lần thiền định. Tinh thần tôi thoái mái hơn rất nhiều."' },
      { user: 'Trịnh Bảo Trâm', text: '"PT luôn lắng nghe và điều chỉnh cường độ sao cho phù hợp với thể trạng của tôi từng ngày."' }
    ]
  },
  {
    id: 3,
    name: 'TUẤN LEE',
    specialty: 'Calisthenics • HIIT',
    price: 400000,
    rating: 4.8,
    reviews: 98,
    experience: '5 năm',
    image: 'https://images.unsplash.com/photo-1517836357463-d25dfeac3438?q=80&w=1470&auto=format&fit=crop',
    avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?q=80&w=1374&auto=format&fit=crop',
    tags: ['Calisthenics', 'HIIT'],
    bio: 'Bậc thầy Calisthenics với kỹ thuật chuẩn xác. Cam kết thay đổi vóc dáng nhanh chóng bằng các bài tập cường độ cao không tạ.',
    comments: [
      { user: 'Đinh Tuấn', text: '"Cực kỳ mệt nhưng cũng cực kỳ phê. HIIT của Tuấn Lee là một đẳng cấp khác!"' },
      { user: 'Lý Tiểu Long', text: '"Kỹ thuật Calisthenics chuẩn chỉ, từ cơ bản đến nâng cao đều được hướng dẫn tận tình."' },
      { user: 'Nguyễn B', text: '"Vóc dáng thay đổi rõ rệt chỉ sau 4 tuần. Rất khắt khe nhưng xứng đáng."' },
      { user: 'Mai Phương', text: '"Bài tập đa dạng, không cần tạ mà vẫn lên cơ ầm ầm. Đỉnh của đỉnh!"' },
      { user: 'Trần Văn C', text: '"Cách truyền lửa của Tuấn rất tốt, luôn đốc thúc những lúc tôi muốn bỏ cuộc."' }
    ]
  }
];

const STEPS = [
  {
    num: '01',
    title: 'Tạo Hồ Sơ',
    desc: 'Nhập mục tiêu tập luyện, thông số cơ thể và thời gian rảnh của bạn vào hệ thống LockedIn.',
  },
  {
    num: '02',
    title: 'Chọn HLV',
    desc: 'Duyệt qua danh sách HLV được xác minh. So sánh chuyên môn, giá cả và đánh giá thực.',
  },
  {
    num: '03',
    title: 'Bắt Đầu Rèn Luyện',
    desc: 'Đặt lịch, thanh toán an toàn qua escrow, và bắt đầu hành trình chinh phục mục tiêu.',
  },
];


// Scroll animation hook
const useScrollAnimation = () => {
  const ref = useRef<HTMLDivElement>(null);
  useEffect(() => {
    const observer = new IntersectionObserver(
      (entries) => {
        entries.forEach((entry) => {
          if (entry.isIntersecting) {
            entry.target.classList.add('visible');
          }
        });
      },
      { threshold: 0.1 }
    );

    const elements = document.querySelectorAll('.animate-on-scroll');
    elements.forEach((el) => observer.observe(el));
    return () => observer.disconnect();
  }, []);
  return ref;
};



const BACKGROUND_VIDEOS = [
  '/videos/4861137-uhd_3840_2160_25fps.mp4',
  '/videos/6388432-uhd_3840_2160_25fps.mp4',
  '/videos/6388870-uhd_3840_2160_25fps.mp4'
];

const Home: React.FC = () => {
  const { currentUser, currentRole } = useAuth();
  useScrollAnimation();

  const [activeSlide, setActiveSlide] = useState(0);
  const [isPaused, setIsPaused] = useState(false);
  const [currentVideo, setCurrentVideo] = useState(0);
  const [progressKey, setProgressKey] = useState(0);

  useEffect(() => {
    if (isPaused) return;
    const ptInterval = setInterval(() => {
      setActiveSlide((prev) => {
        setProgressKey(k => k + 1);
        return (prev + 1) % FEATURED_SHOWCASE.length;
      });
    }, 8000);
    return () => clearInterval(ptInterval);
  }, [isPaused]);

  if (currentUser && currentRole === 'pt') {
    return <PTHome />;
  }

  if (currentUser && currentRole === 'admin') {
    return <AdminHome />;
  }

  return (
    <div className="bg-brand-black min-h-screen mobile-content-pad">
      <style>{`
        @keyframes progress-ring {
          from { stroke-dashoffset: 113.1; }
          to { stroke-dashoffset: 0; }
        }
        @keyframes progress-bar {
          from { transform: scaleX(0); }
          to { transform: scaleX(1); }
        }
        @keyframes marquee-x {
          from { transform: translateX(0); }
          to { transform: translateX(-33.3333%); }
        }
      `}</style>

      {/* ─── HERO SECTION ─── */}
      <section className="relative overflow-hidden pt-12 pb-6 lg:pt-16 lg:pb-10 min-h-[90vh] flex items-center justify-center">

        {/* Background Video Carousel (z-index 0) */}
        <div className="absolute inset-0 z-0 bg-black">
          {BACKGROUND_VIDEOS.map((vid, idx) => {
            const isActive = currentVideo === idx;
            return (
              <video 
                key={vid}
                src={vid} 
                muted 
                autoPlay
                loop={false}
                playsInline
                onEnded={() => isActive && setCurrentVideo((prev) => (prev + 1) % BACKGROUND_VIDEOS.length)}
                onError={() => isActive && setCurrentVideo((prev) => (prev + 1) % BACKGROUND_VIDEOS.length)}
                className={`absolute inset-0 w-full h-full object-cover transition-opacity duration-1000 z-0 ${isActive ? 'opacity-100' : 'opacity-0'}`} 
                ref={el => {
                  if (el) {
                    if (isActive) el.play().catch(() => {});
                    else el.pause();
                  }
                }}
              />
            )
          })}
          
          {/* Black overlay rgba(0,0,0, 0.55) (z-index 1) */}
          <div className="absolute inset-0 bg-black/55 z-10" />
        </div>

        {/* Red vertical accent - hidden since content is centered */}

        {/* Content (z-index 2) */}
        <div className="section-container relative z-20 w-full">
          <div className="flex flex-col items-center text-center justify-center gap-8 max-w-4xl mx-auto pt-2 lg:pt-4">
            
            {/* Label */}
            <div className="flex items-center gap-3 justify-center">
              <div className="w-8 h-px bg-brand-red" />
              <span className="section-label">Kết Nối Huấn Luyện Viên Chuyên Nghiệp</span>
              <div className="w-8 h-px bg-brand-red" />
            </div>

            {/* Headline */}
            <h1 className="font-montserrat font-black text-6xl sm:text-7xl lg:text-8xl text-white uppercase leading-[0.95] tracking-tight">
              ĐÁNH THỨC
              <span className="block text-brand-red">THỂ TRẠNG</span>
              ĐỈNH CAO
            </h1>

            {/* Subtext */}
            <p className="text-white/80 text-lg leading-relaxed max-w-2xl">
              LockedIn kết nối bạn với đội ngũ huấn luyện viên cá nhân chuyên nghiệp và xác minh uy tín. 
              Đồng hành xây dựng lộ trình tập luyện bài bản cùng hệ thống thanh toán an toàn, minh bạch qua ký quỹ escrow.
            </p>

            {/* CTA Group */}
            <div className="flex flex-wrap gap-4 justify-center mt-2">
              <Link to="/marketplace" className="btn-primary text-base py-4 px-8 animate-pulse-red">
                <Zap size={18} fill="white" />
                Tìm HLV Của Bạn
                <ArrowRight size={18} />
              </Link>
              {!currentUser && (
                <Link to="/register" className="btn-secondary text-base py-4 px-8">
                  Đăng Ký Miễn Phí
                </Link>
              )}
            </div>
          </div>
        </div>

        {/* Scroll indicator */}
        <div className="absolute bottom-8 left-1/2 -translate-x-1/2 flex flex-col items-center gap-2 animate-bounce">
          <div className="w-px h-12 bg-gradient-to-b from-white/0 via-white/40 to-white/0" />
        </div>
      </section>



      {/* ─── HOW IT WORKS (CLEAN & ORGANIC) ─── */}
      <section id="how-it-works" className="py-16 lg:py-24 bg-brand-black border-y border-brand-border relative">
        <div className="section-container relative z-10">
          <div className="text-center mb-16">
            <p className="section-label mb-3">Quy Trình Đơn Giản</p>
            <h3 className="font-montserrat font-extrabold text-4xl lg:text-5xl text-white uppercase tracking-wider">
              Bắt Đầu Trong <span className="text-brand-red">3 Bước</span>
            </h3>
          </div>

          <div className="flex flex-col md:flex-row gap-12 md:gap-8 justify-between relative">
            {/* Connecting line for desktop */}
            <div className="hidden md:block absolute top-8 left-[16.66%] right-[16.66%] h-[2px] bg-gradient-to-r from-brand-red/10 via-brand-red/80 to-brand-red/10 rounded-2xl" />

            {STEPS.map((step, i) => (
              <div key={i} className="flex-1 relative group flex flex-col items-center text-center">
                {/* Number Circle */}
                <div className="w-16 h-16 rounded-full bg-brand-surface border border-brand-red/30 flex items-center justify-center text-brand-red font-montserrat font-black text-2xl mb-6 shadow-[0_0_20px_rgba(230,0,0,0.1)] group-hover:shadow-[0_0_30px_rgba(230,0,0,0.4)] group-hover:scale-110 transition-all duration-300 relative z-10">
                  {step.num}
                </div>
                <h4 className="font-montserrat font-bold text-xl text-white uppercase tracking-wider mb-3">
                  {step.title}
                </h4>
                <p className="text-white/60 text-sm leading-relaxed max-w-xs text-center">
                  {step.desc}
                </p>
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* ─── HLV NỔI BẬT SECTION (Spotlight Layout) ─── */}
      <section 
        className="bg-brand-black pt-0 pb-[60px] overflow-hidden relative"
        onMouseEnter={() => setIsPaused(true)}
        onMouseLeave={() => setIsPaused(false)}
      >
        <div className="w-full">
          <div className="flex items-end justify-between mb-8 relative z-10 w-full px-6 lg:px-12 xl:px-[10%]">
            <div>
              <p className="section-label mb-2">Đội Ngũ HLV</p>
              <h3 className="font-montserrat font-extrabold text-3xl lg:text-4xl text-white uppercase tracking-wider">
                HLV Nổi Bật
              </h3>
            </div>
            <Link
              to="/marketplace"
              className="flex items-center gap-2 text-xs lg:text-sm text-white/50 hover:text-white uppercase tracking-widest transition-colors duration-200"
            >
              Xem Tất Cả <ChevronRight size={16} />
            </Link>
          </div>

          {/* SPOTLIGHT FULL-WIDTH CONTAINER */}
          <div className="relative flex flex-col w-full overflow-hidden border-b border-[#1a1a1a]">
            
            {/* PT CONTENT (Fading between PTs) */}
            <div className="relative min-h-[600px] lg:min-h-[650px] 2xl:min-h-[750px] w-full flex-shrink-0">
              {FEATURED_SHOWCASE.map((pt, index) => {
                const isActive = activeSlide === index;
                
                return (
                  <div 
                    key={pt.id} 
                    className={`absolute inset-0 w-full h-full flex flex-col lg:flex-row transition-opacity duration-400 ease-in-out ${isActive ? 'opacity-100 z-10' : 'opacity-0 z-0 pointer-events-none'}`}
                  >
                    {/* Left: Info Column (40%) */}
                    <div className="w-full lg:w-[40%] xl:w-[35%] p-6 lg:py-16 lg:px-12 xl:pl-[10%] flex flex-col justify-between bg-brand-black relative z-20 shrink-0">
                      <div>
                        <div className="flex items-center gap-6 lg:gap-8 mb-6">
                          <div className="w-20 h-20 lg:w-24 lg:h-24 rounded-full border-2 border-brand-red flex items-center justify-center bg-brand-dark shadow-[0_0_15px_rgba(230,51,51,0.3)] shrink-0 overflow-hidden">
                            {pt.avatar ? (
                               <img src={pt.avatar} alt={pt.name} className="w-full h-full object-cover" />
                            ) : (
                               <span className="font-display font-bold text-2xl lg:text-3xl text-white">
                                 {pt.name.split(' ').map(n => n[0]).join('').substring(0,2)}
                               </span>
                            )}
                          </div>
                          <div>
                            <h2 className="text-[20px] font-semibold text-white uppercase tracking-wider leading-tight">
                              HLV {pt.name}
                            </h2>
                            <p className="text-[#666] text-[11px] mt-1 font-semibold tracking-wide">Pro Coach • {pt.experience}</p>
                          </div>
                        </div>

                        <div className="flex flex-wrap gap-2 mb-4">
                          {pt.tags.map((tag, i) => (
                            <span key={i} className="px-3 py-1.5 text-[10px] font-bold uppercase tracking-widest text-brand-red bg-brand-red/10 border border-brand-red/20 rounded-full">
                              {tag}
                            </span>
                          ))}
                        </div>

                        <p className="text-[12px] text-[#666] leading-[1.6]">
                          {pt.bio}
                        </p>
                      </div>

                      <div className="grid grid-cols-3 gap-3 border-t border-[#1e1e1e] pt-6 mt-6 lg:mt-auto">
                         <div className="bg-[#1a1a1a] rounded-[8px] p-2 flex flex-col justify-center items-center text-center">
                           <div className="flex items-center gap-1 mb-1">
                             <Star size={12} className="text-[#e63333] fill-[#e63333]" />
                             <span className="text-[#e63333] font-bold text-[16px] leading-none">{pt.rating}</span>
                           </div>
                           <p className="text-[#555] text-[9px] uppercase font-bold">Đánh giá</p>
                         </div>
                         <div className="bg-[#1a1a1a] rounded-[8px] p-2 flex flex-col justify-center items-center text-center">
                           <span className="text-[#e63333] font-bold text-[16px] leading-none mb-1">{pt.reviews}</span>
                           <p className="text-[#555] text-[9px] uppercase font-bold">Reviews</p>
                         </div>
                         <div className="bg-[#1a1a1a] rounded-[8px] p-2 flex flex-col justify-center items-center text-center">
                           <span className="text-[#e63333] font-bold text-[16px] leading-none mb-1">{pt.price / 1000}k</span>
                           <p className="text-[#555] text-[9px] uppercase font-bold">Buổi</p>
                         </div>
                      </div>
                    </div>

                    {/* Right: Image Column (60%) */}
                    <div className="w-full lg:w-[60%] xl:w-[65%] relative flex flex-col p-6 lg:py-16 lg:px-12 xl:pr-[10%] justify-between shrink-0 overflow-hidden group">
                      {pt.image && (
                        <img 
                          src={pt.image} 
                          alt={pt.name}
                          className="absolute inset-0 w-full h-full object-cover object-center opacity-50 grayscale-[20%] transition-transform duration-[10s] group-hover:scale-105"
                        />
                      )}
                      <div className="absolute inset-0 z-0" style={{ background: 'linear-gradient(135deg, rgba(42,10,10,0.8) 0%, rgba(26,16,16,0.6) 40%, rgba(13,13,13,0.9) 100%)' }}></div>
                      <div className="absolute inset-0 opacity-[0.05] pointer-events-none z-0" style={{ backgroundImage: 'linear-gradient(to right, #ffffff 1px, transparent 1px), linear-gradient(to bottom, #ffffff 1px, transparent 1px)', backgroundSize: '24px 24px' }}></div>
                      
                      <div className="relative z-10 flex">
                        <span className="inline-flex items-center gap-1 bg-black/50 backdrop-blur border border-[#333] px-4 py-2 rounded-full text-xs lg:text-sm font-bold text-white shadow-lg">
                          ★★★★ <span className="text-brand-red ml-1">{pt.rating}</span> <span className="text-white/40 font-normal ml-1">({pt.reviews} đánh giá)</span>
                        </span>
                      </div>

                      <div className="relative z-10 flex justify-end mt-auto">
                        <button className="backdrop-blur-md bg-white/10 border border-white/20 text-white font-bold uppercase tracking-widest text-xs lg:text-sm px-8 py-4 rounded-full hover:bg-white/20 transition-all shadow-[0_0_15px_rgba(255,255,255,0.1)] flex items-center gap-2">
                          Đặt lịch ngay <ArrowRight size={18} />
                        </button>
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>

            {/* MIDDLE SECTION: Testimonial Strip */}
            <div className="w-full bg-[#050505] border-t border-[#1a1a1a] py-6 px-6 lg:px-12 xl:px-[10%] relative overflow-hidden z-20">
              <div className="absolute top-2 left-6 text-[#2a2a2a] opacity-20 font-serif text-6xl pointer-events-none leading-none z-0">"</div>
              
              <div className="relative z-10 w-full overflow-hidden">
                {FEATURED_SHOWCASE.map((pt, index) => {
                  if (activeSlide !== index) return null;
                  
                  const visibleTestimonials = [
                    pt.comments[0],
                    pt.comments[1],
                    pt.comments[2],
                  ];
                  
                  return (
                    <div key={index} className="grid grid-cols-1 md:grid-cols-3 gap-4 lg:gap-6 animate-fade-in w-full">
                       {visibleTestimonials.map((t, i) => (
                         <div key={i} className="bg-[#111] border border-[#2a2a2a] rounded-[10px] p-6 flex flex-col justify-between hover:border-[#444] transition-colors w-full">
                           <p className="text-white/60 text-xs italic leading-relaxed mb-4 line-clamp-3">
                             "{t.text.replace(/"/g, '')}"
                           </p>
                           <p className="text-brand-red text-[10px] font-bold uppercase tracking-wider">— {t.user}</p>
                         </div>
                       ))}
                    </div>
                  );
                })}
              </div>
            </div>

            {/* BOTTOM SECTION: Navigation Bar */}
            <div className="w-full bg-brand-black border-t border-[#1a1a1a] mt-4 py-3 px-6 lg:px-12 xl:px-[10%] flex items-center justify-between relative z-20">
              <div className="flex items-center gap-4">
                {FEATURED_SHOWCASE.map((pt, i) => (
                  <button 
                    key={pt.id}
                    onClick={() => {
                       setActiveSlide(i);
                       setProgressKey(k => k + 1);
                    }}
                    className={`w-12 h-12 md:w-14 md:h-14 rounded-full flex items-center justify-center bg-brand-dark transition-all border-[1.5px] overflow-hidden ${activeSlide === i ? 'border-brand-red shadow-[0_0_10px_rgba(230,51,51,0.4)]' : 'border-[#333] hover:border-[#555] opacity-50 hover:opacity-100'}`}
                  >
                    {pt.avatar ? (
                       <img src={pt.avatar} alt={pt.name} className="w-full h-full object-cover" />
                    ) : (
                       <span className="font-display font-bold text-xs md:text-sm text-white">
                          {pt.name.split(' ').map(n => n[0]).join('').substring(0,2)}
                       </span>
                    )}
                  </button>
                ))}
              </div>

              <div className="flex items-center gap-4 w-[200px] md:w-[250px]">
                <div className="h-[4px] w-full bg-[#222] rounded-full overflow-hidden flex-1">
                  {!isPaused && (
                    <div 
                      key={progressKey}
                      className="h-full bg-brand-red origin-left"
                      style={{ animation: 'progress-bar 8s linear forwards' }}
                    />
                  )}
                </div>
                <span className="text-white/40 text-sm font-bold font-display w-8 text-right shrink-0">
                  {activeSlide + 1} / {FEATURED_SHOWCASE.length}
                </span>
              </div>
            </div>
          </div>
        </div>
      </section>

      <style>{`
        @keyframes scroll-up {
          0% { transform: translateY(0); }
          100% { transform: translateY(-50%); }
        }
      `}</style>
      
      {/* ─── WHY LOCKEDIN ─── */}
      <section className="py-14 lg:py-20">
        <div className="section-container">
          <div className="grid lg:grid-cols-2 gap-16 items-center">
            <div className="animate-on-scroll">
              <p className="section-label mb-4">Tại Sao Chọn Chúng Tôi</p>
              <h2 className="font-montserrat font-extrabold text-5xl lg:text-6xl text-white uppercase tracking-wider mb-8">
                An Toàn.<br />
                <span className="text-brand-red">Minh Bạch.</span><br />
                Hiệu Quả.
              </h2>
              <p className="text-white/50 text-base leading-relaxed mb-8">
                LockedIn không chỉ là ứng dụng tìm HLV. Chúng tôi là người bảo đảm 
                toàn bộ hành trình — từ kết nối, thanh toán escrow, đến theo dõi tiến trình tập luyện chi tiết.
              </p>

              <div className="flex flex-col gap-4">
                {[
                  'HLV được xác minh danh tính và chứng chỉ',
                  'Thanh toán escrow — tiền chỉ giải phóng khi hoàn thành',
                  'Báo cáo tiến độ trực quan để theo dõi kết quả thực tế',
                  'Tranh chấp được Admin giải quyết công bằng',
                ].map((item, i) => (
                  <div key={i} className="flex items-start gap-3">
                    <CheckCircle size={18} className="text-brand-red flex-shrink-0 mt-0.5" />
                    <span className="text-white/70 text-sm">{item}</span>
                  </div>
                ))}
              </div>
            </div>

            {/* Feature cards */}
            <div className="grid grid-cols-2 gap-4 animate-on-scroll">
              {[
                { icon: Award, title: 'HLV Xác Minh', desc: '100% có chứng chỉ chuyên nghiệp' },
                { icon: TrendingUp, title: 'Báo Cáo Tiến Độ', desc: 'Dữ liệu tập luyện chi tiết và trực quan' },
                { icon: Users, title: 'Cộng Đồng', desc: '10,000+ thành viên đang luyện tập' },
                { icon: Zap, title: 'AI Meal Plan', desc: 'Kế hoạch dinh dưỡng cá nhân hóa' },
              ].map((feature, i) => {
                const Icon = feature.icon;
                return (
                  <div key={i} className="card-dark p-6">
                    <div className="w-10 h-10 bg-brand-red rounded-xl flex items-center justify-center mb-4">
                      <Icon size={18} className="text-white" />
                    </div>
                    <h4 className="font-semibold text-white text-sm mb-2">{feature.title}</h4>
                    <p className="text-white/40 text-xs leading-relaxed">{feature.desc}</p>
                  </div>
                );
              })}
            </div>
          </div>
        </div>
      </section>



      {/* ─── FINAL CTA ─── */}
      <section className="py-14 lg:py-20">
        <div className="section-container">
          <div className="py-12 lg:py-20 text-center animate-on-scroll relative overflow-hidden">
            <div className="relative z-10">
              <p className="section-label mb-6">Bắt Đầu Ngay Hôm Nay</p>
              <h2 className="font-montserrat font-black text-6xl lg:text-8xl text-white uppercase tracking-tight mb-6">
                SẴN SÀNG<br />
                <span className="text-brand-red">CHINH PHỤC?</span>
              </h2>
              <p className="text-white/50 text-lg max-w-xl mx-auto mb-10">
                Hàng nghìn người đã bắt đầu hành trình chinh phục vóc dáng của họ. 
                Đây là lúc của bạn.
              </p>
              <div className="flex flex-wrap gap-4 justify-center">
                <Link to="/marketplace" className="btn-primary text-base py-4 px-10">
                  <Zap size={18} fill="white" />
                  Tìm HLV Ngay
                </Link>
                {!currentUser && (
                  <Link to="/register" className="btn-secondary text-base py-4 px-10">
                    Đăng Ký Miễn Phí
                  </Link>
                )}
              </div>
            </div>
          </div>
        </div>
      </section>

      {/* ─── FOOTER ─── */}
      <footer className="border-t border-brand-border py-16 bg-brand-dark/30">
        <div className="section-container">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-12 mb-16">
            {/* Column 1: Logo & Info */}
            <div className="flex flex-col gap-5">
              <div className="flex items-center gap-2">
                <img src={logoImg} alt="LockedIn Logo" className="h-10 w-auto object-contain" />
              </div>
              <p className="text-white/40 text-sm leading-relaxed">
                LockedIn là nền tảng kết nối huấn luyện viên cá nhân chuyên nghiệp và đồng hành tập luyện hàng đầu Việt Nam.
              </p>
              
              {/* Detailed Business & Contact Info */}
              <div className="text-xs text-white/30 flex flex-col gap-2 border-t border-brand-border/40 pt-4 mt-2">
                <p className="leading-relaxed">
                  <strong className="text-white/50">Văn phòng:</strong> Nhà văn hóa sinh viên TP.HCM, Lưu Hữu Phước, Đông Hoà, Dĩ An, Bình Dương, Vietnam
                </p>
                <p>
                  <strong className="text-white/50">Hotline:</strong> <span className="text-brand-red font-mono hover:underline cursor-pointer">+84 33 724 4845</span>
                </p>
                <p>
                  <strong className="text-white/50">Email:</strong> <span className="text-white/60 font-mono hover:underline cursor-pointer">lockedinisyourfriend@gmail.com</span>
                </p>
                <p className="text-[11px] text-white/20 mt-1">
                  GPĐKKD số: 0317896542 do Sở KH&ĐT TP.HCM cấp ngày 12/03/2026
                </p>
              </div>

              {/* Social links */}
              <div className="flex gap-4 mt-2">
                <a
                  href="#"
                  className="w-8 h-8 rounded-full border border-brand-border hover:border-brand-red hover:text-brand-red text-white/40 flex items-center justify-center transition-all duration-200 hover:shadow-[0_0_8px_rgba(255,0,0,0.4)]"
                  title="Facebook"
                >
                  <svg className="w-3.5 h-3.5 fill-current" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                    <path d="M22 12c0-5.52-4.48-10-10-10S2 6.48 2 12c0 4.84 3.44 8.87 8 9.8V15H8v-3h2V9.5C10 7.57 11.57 6 13.5 6H16v3h-2c-.55 0-1 .45-1 1v2h3v3h-3v6.95c4.56-.93 8-4.96 8-9.75z"/>
                  </svg>
                </a>
                <a
                  href="#"
                  className="w-8 h-8 rounded-full border border-brand-border hover:border-brand-red hover:text-brand-red text-white/40 flex items-center justify-center transition-all duration-200 hover:shadow-[0_0_8px_rgba(255,0,0,0.4)]"
                  title="Instagram"
                >
                  <svg className="w-3.5 h-3.5 stroke-current fill-none" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                    <rect x="2" y="2" width="20" height="20" rx="5" ry="5"/>
                    <path d="M16 11.37A4 4 0 1 1 12.63 8 4 4 0 0 1 16 11.37z"/>
                    <line x1="17.5" y1="6.5" x2="17.51" y2="6.5"/>
                  </svg>
                </a>
                <a
                  href="#"
                  className="w-8 h-8 rounded-full border border-brand-border hover:border-brand-red hover:text-brand-red text-white/40 flex items-center justify-center transition-all duration-200 hover:shadow-[0_0_8px_rgba(255,0,0,0.4)]"
                  title="YouTube"
                >
                  <svg className="w-3.5 h-3.5 fill-current" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                    <path d="M23.498 6.163a3.003 3.003 0 0 0-2.11-2.11C19.518 3.5 12 3.5 12 3.5s-7.517 0-9.388.503a3.003 3.003 0 0 0-2.11 2.11C0 8.033 0 12 0 12s0 3.967.502 5.837a3.003 3.003 0 0 0 2.11 2.11c1.871.503 9.388.503 9.388.503s7.518 0 9.388-.503a3.003 3.003 0 0 0 2.11-2.11c.503-1.87 0-5.837 0-5.837s0-3.967-.502-5.837zM9.545 15.568V8.432L15.818 12l-6.273 3.568z"/>
                  </svg>
                </a>
              </div>
            </div>

            {/* Column 2 & 3: Navigation Links */}
            {[
              { title: 'Nền Tảng', links: ['Tìm HLV', 'Bảng Giá', 'Lộ Trình', 'Blog'] },
              { title: 'Hỗ Trợ', links: ['Câu Hỏi Thường Gặp', 'Điều Khoản', 'Bảo Mật', 'Liên Hệ'] },
            ].map((col, i) => (
              <div key={i} className="md:pl-4">
                <p className="text-white font-semibold text-sm uppercase tracking-widest mb-6">{col.title}</p>
                <ul className="flex flex-col gap-3">
                  {col.links.map((link, j) => (
                    <li key={j}>
                      <a href="#" className="text-white/40 text-sm hover:text-white transition-colors duration-200">
                        {link}
                      </a>
                    </li>
                  ))}
                </ul>
              </div>
            ))}

            {/* Column 4: Newsletter */}
            <div>
              <p className="text-white font-semibold text-sm uppercase tracking-widest mb-6">Bản Tin</p>
              <p className="text-white/40 text-xs leading-relaxed mb-4">
                Đăng ký nhận tin tức để cập nhật các bài viết thể hình và dinh dưỡng mới nhất từ LockedIn.
              </p>
              <form onSubmit={(e) => { e.preventDefault(); alert('Cảm ơn bạn đã đăng ký!'); }} className="flex gap-2 mb-6">
                <input
                  type="email"
                  placeholder="Email của bạn..."
                  required
                  className="w-full bg-brand-surface border border-brand-border px-3 py-2 text-xs focus:outline-none focus:border-brand-red text-white"
                />
                <button
                  type="submit"
                  className="bg-brand-red hover:bg-brand-red-dark text-white px-4 py-2 text-xs font-semibold uppercase tracking-wider transition-colors cursor-pointer rounded-xl"
                >
                  GỬI
                </button>
              </form>

              {/* Secure Payment Escrow & Trust Badges */}
              <div className="border-t border-brand-border/40 pt-4 flex flex-col gap-2.5">
                <div className="flex items-center gap-2 text-white/30 hover:text-white/50 transition-colors">
                  <svg className="w-4 h-4 text-brand-red flex-shrink-0" fill="none" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M9 12l2 2 4-4m5.618-4.016A11.955 11.955 0 0112 2.944a11.955 11.955 0 01-8.618 3.04A12.02 12.02 0 003 9c0 5.591 3.824 10.29 9 11.622 5.176-1.332 9-6.03 9-11.622 0-1.042-.133-2.052-.382-3.016z"></path>
                  </svg>
                  <span className="text-[10px] uppercase tracking-wider font-semibold">Giao dịch Escrow an toàn 100%</span>
                </div>
                <div className="flex items-center gap-2 text-white/30 hover:text-white/50 transition-colors">
                  <svg className="w-4 h-4 text-brand-red flex-shrink-0" fill="none" stroke="currentColor" strokeWidth="2.5" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg">
                    <path strokeLinecap="round" strokeLinejoin="round" d="M12 15v2m-6 4h12a2 2 0 002-2v-6a2 2 0 00-2-2H6a2 2 0 00-2 2v6a2 2 0 002 2zm10-10V7a4 4 0 00-8 0v4h8z"></path>
                  </svg>
                  <span className="text-[10px] uppercase tracking-wider font-semibold">Bảo mật thông tin chuẩn SSL</span>
                </div>
              </div>
            </div>
          </div>

          <div className="border-t border-brand-border pt-8 flex flex-wrap gap-4 justify-between items-center">
            <p className="text-white/20 text-xs">© 2026 LockedIn. All rights reserved.</p>
          </div>
        </div>
      </footer>

      {/* Mobile Bottom Navigation */}
      <nav className="mobile-bottom-bar">
        {[
          { label: 'Trang Chủ', icon: <HomeIcon size={20} />, path: '/' },
          { label: 'Tìm PT', icon: <Search size={20} />, path: '/marketplace' },
          { label: 'Lịch', icon: <Calendar size={20} />, path: '/customer/bookings' },
          { label: 'Chat', icon: <MessageSquare size={20} />, path: '/customer/workspace' },
          { label: 'Hồ Sơ', icon: <User size={20} />, path: '/customer/profile' },
        ].map((tab, i) => (
          <Link
            key={i}
            to={tab.path}
            className="flex flex-col items-center gap-1 px-3 py-1 text-white/50 hover:text-brand-red transition-colors"
          >
            <span className="flex items-center justify-center">{tab.icon}</span>
            <span className="text-[10px] uppercase tracking-widest mt-1">{tab.label}</span>
          </Link>
        ))}
      </nav>
    </div>
  );
};

export default Home;
