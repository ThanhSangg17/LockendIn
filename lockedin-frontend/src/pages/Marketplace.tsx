// src/pages/Marketplace.tsx
import React, { useState, useMemo, useEffect } from 'react';
import { Search, SlidersHorizontal, Star, ArrowRight, ChevronDown, Home, Calendar, MessageSquare, User } from 'lucide-react';
import { Link } from 'react-router-dom';
import Badge from '../components/Badge';
import api from '../services/api';

const SPECIALTIES = ['Tất Cả', 'Tăng Cơ', 'Giảm Cân', 'Yoga', 'CrossFit', 'Cardio', 'Thể Hình', 'Calisthenics'];

const StarRating: React.FC<{ rating: number; size?: number }> = ({ rating, size = 12 }) => (
  <div className="flex gap-0.5">
    {Array.from({ length: 5 }).map((_, i) => (
      <Star key={i} size={size} className={i < Math.floor(rating) ? 'text-white fill-white' : 'text-white/20 fill-transparent'} />
    ))}
  </div>
);

const Marketplace: React.FC = () => {
  // const { createBooking } = useData();
  const [pts, setPts] = useState<any[]>([]);
  const [search, setSearch] = useState('');
  const [selectedSpecialty, setSelectedSpecialty] = useState('Tất Cả');
  const [priceMax, setPriceMax] = useState(700000);
  const [sortBy, setSortBy] = useState<'rating' | 'price_asc' | 'price_desc' | 'reviews'>('rating');
  const [filterOpen, setFilterOpen] = useState(false);
  /*
  const [selectedPT, setSelectedPT] = useState<any | null>(null);
  const [ptPackages, setPtPackages] = useState<any[]>([]);
  const [selectedPackage, setSelectedPackage] = useState<any | null>(null);
  */

  useEffect(() => {
    const fetchPts = async () => {
      try {
        const res = await api.get('/marketplace/pts');
        if (res.data?.success) {
          const items = res.data.data.items || [];
          const mapped = items.map((p: any) => ({
            id: p.ptProfileId,
            name: p.fullName,
            specialty: p.specialization || 'Huấn Luyện Viên',
            rating: p.averageRating,
            reviews: p.totalReviews,
            price: 300000, 
            experience: p.experienceYears,
            location: 'Việt Nam',
            initials: p.fullName.split(' ').map((n: string) => n[0]).join('').toUpperCase().slice(0, 2),
            available: true,
            certified: true,
            tags: p.specialization ? p.specialization.split(',').map((t: string) => t.trim()) : []
          }));
          
          if (mapped.length === 0) {
            const mockPTs = [
              {
                id: 'pt-mock-1',
                name: 'PT Test Account',
                specialty: 'Tăng Cơ & Sức Mạnh',
                rating: 4.9,
                reviews: 142,
                price: 300000,
                experience: 8,
                location: 'Việt Nam',
                initials: 'AR',
                available: true,
                certified: true,
                tags: ['Tăng Cơ', 'Thể Hình', 'Sức Mạng']
              },
              {
                id: 'pt-mock-2',
                name: 'Trần Thị Lan',
                specialty: 'Giảm Cân & Cardio',
                rating: 4.8,
                reviews: 98,
                price: 280000,
                experience: 5,
                location: 'Việt Nam',
                initials: 'TL',
                available: true,
                certified: true,
                tags: ['Giảm Cân', 'Cardio', 'HIIT']
              },
              {
                id: 'pt-mock-3',
                name: 'Phạm Minh Đức',
                specialty: 'Yoga & Phục Hồi',
                rating: 5.0,
                reviews: 76,
                price: 350000,
                experience: 6,
                location: 'Việt Nam',
                initials: 'MĐ',
                available: true,
                certified: true,
                tags: ['Yoga', 'Phục Hồi', 'Dẻo Dai']
              }
            ];
            setPts(mockPTs);
          } else {
            setPts(mapped);
          }
        } else {
          throw new Error('marketplace failed');
        }
      } catch (e) {
        console.error("Failed to load PTs, loading mock data:", e);
        const mockPTs = [
          {
            id: 'pt-mock-1',
            name: 'PT Test Account',
            specialty: 'Tăng Cơ & Sức Mạnh',
            rating: 4.9,
            reviews: 142,
            price: 300000,
            experience: 8,
            location: 'Việt Nam',
            initials: 'AR',
            available: true,
            certified: true,
            tags: ['Tăng Cơ', 'Thể Hình', 'Sức Mạng']
          },
          {
            id: 'pt-mock-2',
            name: 'Trần Thị Lan',
            specialty: 'Giảm Cân & Cardio',
            rating: 4.8,
            reviews: 98,
            price: 280000,
            experience: 5,
            location: 'Việt Nam',
            initials: 'TL',
            available: true,
            certified: true,
            tags: ['Giảm Cân', 'Cardio', 'HIIT']
          },
          {
            id: 'pt-mock-3',
            name: 'Phạm Minh Đức',
            specialty: 'Yoga & Phục Hồi',
            rating: 5.0,
            reviews: 76,
            price: 350000,
            experience: 6,
            location: 'Việt Nam',
            initials: 'MĐ',
            available: true,
            certified: true,
            tags: ['Yoga', 'Phục Hồi', 'Dẻo Dai']
          }
        ];
        setPts(mockPTs);
      }
    };
    fetchPts();
  }, []);

  /*
  const handleOpenBooking = async (pt: any) => {
    setSelectedPT(pt);
    setPtPackages([]);
    setSelectedPackage(null);
    try {
      const res = await api.get(`/marketplace/pts/${pt.id}/packages`);
      if (res.data?.success) {
        setPtPackages(res.data.data);
        if (res.data.data.length > 0) {
          setSelectedPackage(res.data.data[0]);
        }
      }
    } catch (e) {
      console.error("Failed to load packages for PT:", e);
    }
  };
  */

  const filtered = useMemo(() => {
    let list = [...pts];
    if (search) {
      const q = search.toLowerCase();
      list = list.filter((p) => p.name.toLowerCase().includes(q) || p.specialty.toLowerCase().includes(q) || p.tags.some((t: string) => t.toLowerCase().includes(q)));
    }
    if (selectedSpecialty !== 'Tất Cả') {
      list = list.filter((p) => p.specialty.toLowerCase().includes(selectedSpecialty.toLowerCase()) || p.tags.some((t: string) => t.toLowerCase().includes(selectedSpecialty.toLowerCase())));
    }
    list = list.filter((p) => p.price <= priceMax);

    switch (sortBy) {
      case 'rating': list.sort((a, b) => b.rating - a.rating); break;
      case 'price_asc': list.sort((a, b) => a.price - b.price); break;
      case 'price_desc': list.sort((a, b) => b.price - a.price); break;
      case 'reviews': list.sort((a, b) => b.reviews - a.reviews); break;
    }
    return list;
  }, [pts, search, selectedSpecialty, priceMax, sortBy]);

  return (
    <div className="min-h-screen bg-brand-black mobile-content-pad">
      {/* Page Header */}
      <div className="border-b border-brand-border bg-brand-dark">
        <div className="section-container py-12">
          <p className="section-label mb-3">Marketplace</p>
          <h1 className="page-title mb-2">
            Tìm Huấn Luyện Viên<br />
            <span className="text-brand-red">Hoàn Hảo Cho Bạn</span>
          </h1>
          <p className="text-white/40 text-sm mt-4">
            {filtered.length} huấn luyện viên được tìm thấy
          </p>
        </div>
      </div>

      <div className="section-container py-8">
        {/* Search & Controls Row */}
        <div className="flex flex-col md:flex-row gap-4 mb-8">
          {/* Search */}
          <div className="relative flex-1">
            <Search size={16} className="absolute left-4 top-1/2 -translate-y-1/2 text-white/30" />
            <input
              type="text"
              placeholder="Tìm theo tên, chuyên môn, kỹ năng..."
              value={search}
              onChange={(e) => setSearch(e.target.value)}
              className="input-dark pl-11"
            />
          </div>

          {/* Sort Dropdown */}
          <div className="relative">
            <select
              value={sortBy}
              onChange={(e) => setSortBy(e.target.value as typeof sortBy)}
              className="input-dark pr-10 appearance-none cursor-pointer min-w-[180px]"
            >
              <option value="rating">Đánh Giá Cao Nhất</option>
              <option value="price_asc">Giá Thấp → Cao</option>
              <option value="price_desc">Giá Cao → Thấp</option>
              <option value="reviews">Nhiều Đánh Giá Nhất</option>
            </select>
            <ChevronDown size={14} className="absolute right-3 top-1/2 -translate-y-1/2 text-white/40 pointer-events-none" />
          </div>

          {/* Filter Toggle (mobile) */}
          <button
            onClick={() => setFilterOpen(!filterOpen)}
            className="md:hidden btn-secondary text-xs py-3 gap-2"
          >
            <SlidersHorizontal size={14} />
            Bộ Lọc
          </button>
        </div>

        <div className="flex gap-8">
          {/* Filter Sidebar */}
          <aside className={`${filterOpen ? 'block' : 'hidden'} md:block w-full md:w-64 flex-shrink-0`}>
            <div className="bg-brand-surface border border-brand-border p-6 sticky top-20">
              <h3 className="font-montserrat font-bold text-lg text-white uppercase tracking-wider mb-6">
                Bộ Lọc
              </h3>

              {/* Specialty Filter */}
              <div className="mb-8">
                <p className="text-white/40 text-xs uppercase tracking-widest mb-4">Chuyên Môn</p>
                <div className="flex flex-col gap-1">
                  {SPECIALTIES.map((spec) => (
                    <button
                      key={spec}
                      onClick={() => setSelectedSpecialty(spec)}
                      className={`text-left px-3 py-2.5 text-sm transition-all duration-200 cursor-pointer ${
                        selectedSpecialty === spec
                          ? 'bg-brand-red text-white font-semibold'
                          : 'text-white/50 hover:text-white hover:bg-white/5'
                      }`}
                    >
                      {spec}
                    </button>
                  ))}
                </div>
              </div>

              {/* Price Range */}
              <div className="mb-8">
                <p className="text-white/40 text-xs uppercase tracking-widest mb-4">Giá Tối Đa</p>
                <input
                  type="range"
                  min="200000"
                  max="700000"
                  step="50000"
                  value={priceMax}
                  onChange={(e) => setPriceMax(Number(e.target.value))}
                  className="w-full accent-brand-red cursor-pointer"
                />
                <div className="flex justify-between mt-2">
                  <span className="text-white/30 text-xs">200k</span>
                  <span className="text-white font-semibold text-sm">{(priceMax / 1000).toFixed(0)}k/buổi</span>
                </div>
              </div>

              {/* Reset */}
              <button
                onClick={() => { setSearch(''); setSelectedSpecialty('Tất Cả'); setPriceMax(700000); setSortBy('rating'); }}
                className="w-full btn-secondary text-xs py-2"
              >
                Xóa Bộ Lọc
              </button>
            </div>
          </aside>

          {/* PT Grid */}
          <main className="flex-1">
            {filtered.length === 0 ? (
              <div className="flex flex-col items-center justify-center py-24 text-center">
                <div className="w-16 h-16 border-2 border-brand-border flex items-center justify-center mb-6">
                  <Search size={24} className="text-white/20" />
                </div>
                <p className="font-montserrat font-extrabold text-2xl text-white uppercase tracking-wider mb-2">Không Tìm Thấy</p>
                <p className="text-white/30 text-sm">Thử điều chỉnh bộ lọc của bạn</p>
              </div>
            ) : (
              <div className="grid sm:grid-cols-2 xl:grid-cols-3 gap-5">
                {filtered.map((pt) => (
                  <div key={pt.id} className="card-dark group flex flex-col overflow-hidden">
                    {/* Status bar */}
                    <div className={`h-0.5 w-full ${pt.available ? 'bg-white' : 'bg-brand-muted'}`} />

                    {/* Card body */}
                    <div className="p-6 flex-1 flex flex-col gap-4">
                      {/* Avatar + basic info */}
                      <div className="flex items-start gap-4">
                        <div className="relative flex-shrink-0">
                          <div className="w-14 h-14 bg-brand-dark border border-brand-border group-hover:border-brand-red transition-colors duration-300 flex items-center justify-center">
                            <span className="font-display text-xl text-white">{pt.initials}</span>
                          </div>
                          {pt.available && (
                            <div className="absolute -bottom-1 -right-1 w-3.5 h-3.5 bg-white border-2 border-black rounded-full" />
                          )}
                        </div>
                        <div className="flex-1 min-w-0">
                          <div className="flex items-center gap-2 mb-1">
                            <h3 className="font-semibold text-white text-sm truncate">{pt.name}</h3>
                            {pt.certified && <span className="w-4 h-4 bg-brand-red flex items-center justify-center flex-shrink-0 rounded-full" title="Đã xác minh">✓</span>}
                          </div>
                          <p className="text-white/40 text-xs mb-2">{pt.experience} năm · {pt.location}</p>
                          <div className="flex items-center gap-2">
                            <StarRating rating={pt.rating} />
                            <span className="text-white text-xs font-semibold">{pt.rating}</span>
                            <span className="text-white/30 text-xs">({pt.reviews})</span>
                          </div>
                        </div>
                      </div>

                      {/* Specialty */}
                      <div>
                        <Badge variant="red" className="mb-3">{pt.specialty}</Badge>
                        <div className="flex flex-wrap gap-1.5">
                          {pt.tags.map((tag: string) => (
                            <span key={tag} className="text-[10px] text-white/40 border border-brand-border px-2 py-0.5 uppercase tracking-wider">
                              {tag}
                            </span>
                          ))}
                        </div>
                      </div>

                      {/* Price */}
                      <div className="mt-auto pt-4 border-t border-brand-border flex items-center justify-between">
                        <div>
                          <p className="text-white/30 text-[10px] uppercase tracking-wider">Giá / Buổi</p>
                          <p className="text-white font-bold text-lg">{pt.price.toLocaleString('vi-VN')}đ</p>
                        </div>
                        <span className={`text-[10px] uppercase tracking-wider font-semibold ${pt.available ? 'text-white' : 'text-brand-muted'}`}>
                          {pt.available ? '● Có Thể Đặt' : '○ Đầy Lịch'}
                        </span>
                      </div>
                    </div>

                    {/* Book CTA */}
                    <Link
                      to={`/marketplace/pt/${pt.id}`}
                      className={`w-full py-3.5 text-xs font-semibold uppercase tracking-widest transition-all duration-200 flex items-center justify-center gap-2 text-center ${
                        pt.available
                          ? 'bg-brand-red text-white hover:bg-brand-red-dark cursor-pointer'
                          : 'bg-brand-border text-white/20 cursor-not-allowed pointer-events-none'
                      }`}
                    >
                      {pt.available ? (
                        <><span>Xem Chi Tiết & Đặt Lịch</span><ArrowRight size={14} /></>
                      ) : (
                        <span>Hết Chỗ</span>
                      )}
                    </Link>
                  </div>
                ))}
              </div>
            )}
          </main>
        </div>
      </div>

      {/* Booking Modal
      {selectedPT && (
        <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm">
          <div className="w-full max-w-md bg-brand-dark border border-brand-border animate-fade-up">
            <div className="h-0.5 w-full bg-brand-red" />
            <div className="flex items-center justify-between px-6 py-4 border-b border-brand-border">
              <h3 className="font-montserrat font-bold text-xl text-white uppercase tracking-widest">Đặt Lịch</h3>
              <button onClick={() => setSelectedPT(null)} className="text-white/40 hover:text-white transition-colors cursor-pointer">
                <X size={20} />
              </button>
            </div>
            <div className="p-6">
              <div className="flex items-center gap-4 mb-6 p-4 bg-brand-surface border border-brand-border">
                <div className="w-12 h-12 bg-brand-dark border border-brand-red flex items-center justify-center">
                  <span className="font-display text-lg text-white">{selectedPT.initials}</span>
                </div>
                <div>
                  <p className="text-white font-semibold">{selectedPT.name}</p>
                  <p className="text-white/40 text-xs">{selectedPT.specialty}</p>
                </div>
                <div className="ml-auto text-right">
                  <p className="text-white font-bold">{selectedPT.price.toLocaleString('vi-VN')}đ</p>
                  <p className="text-white/30 text-xs">/ buổi</p>
                </div>
              </div>

              <div className="flex flex-col gap-4">
                <div>
                  <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Chọn Ngày</label>
                  <input type="date" className="input-dark" defaultValue={new Date().toISOString().slice(0, 10)} />
                </div>
                <div>
                  <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Chọn Giờ</label>
                  <select className="input-dark">
                    {['07:00', '08:00', '09:00', '10:00', '15:00', '16:00', '17:00', '18:00', '19:00'].map((t) => (
                      <option key={t} value={t}>{t}</option>
                    ))}
                  </select>
                </div>
                <div>
                  <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Chọn Gói Tập</label>
                  {ptPackages.length === 0 ? (
                    <p className="text-white/40 text-xs py-2">HLV chưa tạo gói tập nào.</p>
                  ) : (
                    <select
                       className="input-dark"
                       value={selectedPackage?.id || ''}
                       onChange={(e) => {
                         const pkg = ptPackages.find(p => p.id === e.target.value);
                         setSelectedPackage(pkg);
                       }}
                    >
                      {ptPackages.map((p) => (
                        <option key={p.id} value={p.id}>{p.name} — {p.sessionCount} buổi — {p.price.toLocaleString('vi-VN')}đ</option>
                      ))}
                    </select>
                  )}
                </div>
              </div>

              <div className="mt-6 pt-6 border-t border-brand-border flex gap-3">
                <button onClick={() => setSelectedPT(null)} className="btn-secondary flex-1 text-xs py-3">Hủy</button>
                <button
                  disabled={!selectedPackage}
                  onClick={async () => {
                    if (selectedPackage) {
                      try {
                        const booking = await createBooking(selectedPackage);
                        setSelectedPT(null);
                        navigate(`/checkout/${booking.id}`);
                      } catch (e: any) {
                        alert("Không tạo được đơn đặt: " + e.message);
                      }
                    }
                  }}
                  className={`btn-primary flex-1 text-xs py-3 text-center cursor-pointer ${!selectedPackage ? 'opacity-50 cursor-not-allowed' : ''}`}
                >
                  Xác Nhận & Thanh Toán
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
      */}

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

export default Marketplace;
