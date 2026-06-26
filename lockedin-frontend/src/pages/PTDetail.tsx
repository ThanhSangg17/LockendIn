// src/pages/PTDetail.tsx
import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { ArrowLeft, Star, Shield, Loader2, Calendar } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useData } from '../context/DataContext';
import api from '../services/api';
import Badge from '../components/Badge';

const StarRating: React.FC<{ rating: number; size?: number }> = ({ rating, size = 16 }) => (
  <div className="flex gap-1">
    {Array.from({ length: 5 }).map((_, i) => (
      <Star
        key={i}
        size={size}
        className={i < Math.floor(rating) ? 'text-brand-red fill-brand-red' : 'text-white/20 fill-transparent'}
      />
    ))}
  </div>
);

const PTDetail: React.FC = () => {
  const { ptId } = useParams();
  const navigate = useNavigate();
  const { currentUser } = useAuth();
  const { createBooking } = useData();

  const [pt, setPt] = useState<any>(null);
  const [packages, setPackages] = useState<any[]>([]);
  const [reviews, setReviews] = useState<any[]>([]);
  
  const [loading, setLoading] = useState(true);
  const [bookingLoading, setBookingLoading] = useState<string | null>(null);
  const [error, setError] = useState('');

  useEffect(() => {
    const fetchPtDetails = async () => {
      if (!ptId) return;
      setLoading(true);
      setError('');
      try {
        let ptData: any = null;
        let pkgData: any[] = [];
        let revData: any[] = [];

        if (ptId.startsWith('pt-mock-')) {
          if (ptId === 'pt-mock-1') {
            ptData = {
              ptProfileId: 'pt-mock-1',
              fullName: 'PT Test Account',
              specialization: 'Tăng Cơ, Thể Hình, Sức Mạnh',
              experienceYears: 8,
              bio: 'Tôi có hơn 8 năm kinh nghiệm cá nhân chuyên sâu về nâng tạ, tối ưu dinh dưỡng và xây dựng lối sống bền vững. Đã giúp hơn 200 học viên đạt được thân hình mơ ước.',
              averageRating: 4.9,
              totalReviews: 142
            };
            pkgData = [
              {
                id: 'pkg-mock-1',
                ptProfileId: 'pt-mock-1',
                name: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
                description: 'Lộ trình huấn thiện chuyên sâu 1-kèm-1 trong 12 buổi, tập trung vào các kỹ thuật nâng tạ chuẩn, dinh dưỡng tăng cơ và tối ưu hóa phục hồi.',
                sessionCount: 12,
                price: 3600000,
                isActive: true
              },
              {
                id: 'pkg-mock-1-2',
                ptProfileId: 'pt-mock-1',
                name: 'Duy Trì Thể Lực Hàng Tháng',
                description: '4 buổi tập 1-kèm-1 mỗi tháng giúp bạn duy trì thói quen tập luyện và kiểm tra thể trạng thường xuyên.',
                sessionCount: 4,
                price: 1500000,
                isActive: true
              }
            ];
            revData = [
              { id: 'rev-mock-1', rating: 5, comment: 'HLV chỉ dẫn nhiệt tình, giáo án rất khoa học!', createdAt: new Date().toISOString() },
              { id: 'rev-mock-2', rating: 4, comment: 'Rất chuyên nghiệp, giúp tôi tăng 5kg cơ sau 2 tháng.', createdAt: new Date().toISOString() }
            ];
          } else if (ptId === 'pt-mock-2') {
            ptData = {
              ptProfileId: 'pt-mock-2',
              fullName: 'Trần Thị Lan',
              specialization: 'Giảm Cân, Cardio, HIIT',
              experienceYears: 5,
              bio: 'Chuyên gia thiết kế các chương trình tập luyện đốt mỡ hiệu quả kết hợp dinh dưỡng lành mạnh cho phái nữ.',
              averageRating: 4.8,
              totalReviews: 98
            };
            pkgData = [
              {
                id: 'pkg-mock-2',
                ptProfileId: 'pt-mock-2',
                name: 'Giảm Mỡ & Cardio Đốt Calo',
                description: 'Chương trình cardio cường độ cao (HIIT) kết hợp tạ kháng lực nhẹ giúp đốt mỡ thừa tối ưu trong 24 buổi, kèm theo thực đơn dinh dưỡng chi tiết.',
                sessionCount: 24,
                price: 6800000,
                isActive: true
              }
            ];
            revData = [
              { id: 'rev-mock-3', rating: 5, comment: 'Bài tập rất mệt nhưng hiệu quả cực kỳ nhanh, chị Lan siêu tâm lý.', createdAt: new Date().toISOString() }
            ];
          } else {
            ptData = {
              ptProfileId: 'pt-mock-3',
              fullName: 'Phạm Minh Đức',
              specialization: 'Yoga, Phục Hồi, Dẻo Dai',
              experienceYears: 6,
              bio: 'Huấn luyện Yoga trị liệu, cân bằng tâm trí và phục hồi sức khỏe thể chất.',
              averageRating: 5.0,
              totalReviews: 76
            };
            pkgData = [
              {
                id: 'pkg-mock-3',
                ptProfileId: 'pt-mock-3',
                name: 'Yoga Trị Liệu & Cân Bằng',
                description: '10 buổi Yoga kèm giảng dạy chuyên sâu giúp giảm đau vai gáy, cải thiện cột sống và dẻo dai khớp.',
                sessionCount: 10,
                price: 3200000,
                isActive: true
              }
            ];
            revData = [];
          }
          setPt(ptData);
          setPackages(pkgData);
          setReviews(revData);
        } else {
          const ptRes = await api.get(`/marketplace/pts/${ptId}`);
          if (ptRes.data?.success && ptRes.data.data) {
            setPt(ptRes.data.data);
            const pkgRes = await api.get(`/marketplace/pts/${ptId}/packages`);
            if (pkgRes.data?.success) {
              setPackages(pkgRes.data.data || []);
            }
            const revRes = await api.get(`/marketplace/pts/${ptId}/reviews`);
            if (revRes.data?.success) {
              setReviews(revRes.data.data || []);
            }
          } else {
            throw new Error('PT details failed');
          }
        }
      } catch (err: any) {
        console.error('Failed to load PT detail, fallback to mock:', err);
        setPt({
          ptProfileId: ptId,
          fullName: 'PT Test Account',
          specialization: 'Tăng Cơ, Thể Hình, Sức Mạnh',
          experienceYears: 8,
          bio: 'Tôi có hơn 8 năm kinh nghiệm cá nhân chuyên sâu về nâng tạ. Đã giúp hơn 200 học viên đạt được thân hình mơ ước.',
          averageRating: 4.9,
          totalReviews: 142
        });
        setPackages([
          {
            id: 'pkg-mock-1',
            ptProfileId: ptId,
            name: 'Tăng Cơ & Sức Mạnh Cấp Tốc',
            description: 'Lộ trình huấn luyện chuyên sâu 1-kèm-1 trong 12 buổi, tập trung vào các kỹ thuật nâng tạ chuẩn, dinh dưỡng tăng cơ và tối ưu hóa phục hồi.',
            sessionCount: 12,
            price: 3600000,
            isActive: true
          }
        ]);
        setReviews([
          { id: 'rev-mock-1', rating: 5, comment: 'HLV chỉ dẫn nhiệt tình, giáo án rất khoa học!', createdAt: new Date().toISOString() }
        ]);
      } finally {
        setLoading(false);
      }
    };

    fetchPtDetails();
  }, [ptId]);

  const handleSelectPackage = async (pkg: any) => {
    if (!currentUser) {
      alert('Vui lòng đăng nhập để đặt lịch HLV này!');
      navigate('/login');
      return;
    }

    if (currentUser.role !== 'customer') {
      alert('Chỉ tài khoản học viên mới có thể đặt lịch huấn luyện viên.');
      return;
    }

    setBookingLoading(pkg.id);
    try {
      // Map to correct parameter for context method
      const mappedPkg = {
        id: pkg.id,
        ptId: pkg.ptProfileId,
        ptName: pt.fullName,
        name: pkg.name,
        description: pkg.description,
        sessionsCount: pkg.sessionCount,
        price: pkg.price,
        isActive: pkg.isActive
      };
      const booking = await createBooking(mappedPkg);
      navigate(`/checkout/${booking.id}`);
    } catch (err: any) {
      console.error('Failed to book package:', err);
      alert('Đặt lịch thất bại: ' + (err.response?.data?.message || err.message));
    } finally {
      setBookingLoading(null);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-brand-black flex items-center justify-center p-8">
        <div className="text-center text-white/40 flex flex-col items-center gap-2">
          <Loader2 className="animate-spin text-brand-red" size={32} />
          <span className="text-sm uppercase tracking-wider mt-2">Đang tải hồ sơ HLV...</span>
        </div>
      </div>
    );
  }

  if (error || !pt) {
    return (
      <div className="min-h-screen bg-brand-black flex items-center justify-center p-8">
        <div className="text-center max-w-md">
          <p className="text-brand-red text-lg mb-4">{error || 'Có lỗi xảy ra'}</p>
          <button onClick={() => navigate('/marketplace')} className="btn-secondary text-xs py-3 px-6">
            Quay Lại Marketplace
          </button>
        </div>
      </div>
    );
  }

  const initials = pt.fullName.split(' ').map((n: string) => n[0]).join('').substring(0, 2).toUpperCase();
  const specialties = pt.specialization ? pt.specialization.split(',').map((t: string) => t.trim()) : [];

  return (
    <div className="min-h-screen bg-brand-black mobile-content-pad pb-16">
      {/* Upper header */}
      <div className="border-b border-brand-border bg-brand-dark">
        <div className="section-container py-6">
          <button
            onClick={() => navigate('/marketplace')}
            className="flex items-center gap-2 text-white/40 hover:text-white text-xs uppercase tracking-widest mb-8 cursor-pointer transition-colors duration-200"
          >
            <ArrowLeft size={14} />
            Quay Lại Marketplace
          </button>

          <div className="flex flex-col md:flex-row gap-6 items-start md:items-center">
            {/* Avatar */}
            <div className="w-24 h-24 bg-brand-surface border-2 border-brand-red flex items-center justify-center flex-shrink-0">
              <span className="font-display font-bold text-4xl text-white">{initials}</span>
            </div>

            {/* Basic Info */}
            <div className="flex-1 min-w-0">
              <div className="flex items-center gap-3 mb-2 flex-wrap">
                <h1 className="font-montserrat font-black text-3xl sm:text-4xl text-white uppercase tracking-tight">
                  {pt.fullName}
                </h1>
                <Badge variant="red">Verified PT</Badge>
              </div>

              <p className="text-white/40 text-sm mb-3">
                {pt.experienceYears} năm kinh nghiệm · Việt Nam
              </p>

              <div className="flex items-center gap-3">
                <StarRating rating={pt.AverageRating || pt.averageRating || 0} />
                <span className="text-white text-sm font-semibold">{pt.AverageRating || pt.averageRating || 0}</span>
                <span className="text-white/40 text-xs">({pt.TotalReviews || pt.totalReviews || 0} đánh giá)</span>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="section-container py-10">
        <div className="grid lg:grid-cols-3 gap-8 items-start">
          
          {/* Bio and Tags */}
          <div className="lg:col-span-2 flex flex-col gap-8">
            <div className="bg-brand-surface border border-brand-border p-6 sm:p-8">
              <h3 className="font-montserrat font-bold text-xl text-white uppercase tracking-wider mb-4">
                Giới Thiệu Bản Thân
              </h3>
              <p className="text-white/70 text-sm leading-relaxed whitespace-pre-line">
                {pt.bio || 'Chưa có thông tin giới thiệu chi tiết.'}
              </p>

              {specialties.length > 0 && (
                <div className="mt-6 pt-6 border-t border-brand-border">
                  <p className="text-white/40 text-xs uppercase tracking-widest mb-3">Chuyên Môn & Kỹ Năng</p>
                  <div className="flex flex-wrap gap-2">
                    {specialties.map((spec: string, idx: number) => (
                      <span key={idx} className="text-xs text-white/80 border border-brand-border bg-brand-dark px-3 py-1 uppercase tracking-wider">
                        {spec}
                      </span>
                    ))}
                  </div>
                </div>
              )}
            </div>

            {/* Escrow Guarantee Banner */}
            <div className="bg-brand-red/5 border border-brand-red p-6 flex gap-4">
              <div className="w-10 h-10 bg-brand-red flex items-center justify-center flex-shrink-0">
                <Shield size={20} className="text-white" />
              </div>
              <div>
                <h4 className="text-white font-semibold text-sm uppercase tracking-wider mb-1">
                  Đảm Bảo Thanh Toán Escrow Ký Quỹ
                </h4>
                <p className="text-white/50 text-xs leading-relaxed">
                  LockedIn bảo vệ học viên bằng cách giữ tiền thanh toán trong ví Escrow trung gian. 
                  Tiền chỉ được thanh toán cho HLV sau khi mỗi buổi tập được hoàn thành thực tế và 
                  được bạn xác nhận. An toàn 100%.
                </p>
              </div>
            </div>

            {/* Reviews List */}
            <div className="bg-brand-surface border border-brand-border p-6 sm:p-8">
              <h3 className="font-montserrat font-bold text-xl text-white uppercase tracking-wider mb-6">
                Đánh Giá Từ Học Viên ({reviews.length})
              </h3>
              
              {reviews.length === 0 ? (
                <p className="text-white/30 text-sm py-6 text-center">Chưa có nhận xét nào dành cho HLV này.</p>
              ) : (
                <div className="flex flex-col gap-6 divide-y divide-brand-border">
                  {reviews.map((rev, idx) => (
                    <div key={rev.id} className={`pt-6 ${idx === 0 ? 'pt-0' : ''}`}>
                      <div className="flex items-center justify-between mb-2">
                        <span className="text-white font-semibold text-sm">Học viên ẩn danh</span>
                        <span className="text-white/20 text-xs flex items-center gap-1">
                          <Calendar size={11} />
                          {new Date(rev.createdAt).toLocaleDateString('vi-VN')}
                        </span>
                      </div>
                      <div className="mb-2">
                        <StarRating rating={rev.rating} size={12} />
                      </div>
                      <p className="text-white/70 text-sm leading-relaxed">
                        {rev.comment}
                      </p>
                      {rev.ptReply && (
                        <div className="mt-3 p-4 bg-brand-dark border-l-2 border-brand-red text-xs">
                          <p className="text-brand-red font-semibold mb-1">HLV phản hồi:</p>
                          <p className="text-white/60">{rev.ptReply}</p>
                        </div>
                      )}
                    </div>
                  ))}
                </div>
              )}
            </div>
          </div>

          {/* Packages Proposed Grid */}
          <div className="lg:col-span-1 flex flex-col gap-6">
            <h3 className="font-montserrat font-bold text-xl text-white uppercase tracking-wider">
              Các Gói Tập Đề Xuất
            </h3>
            
            {packages.length === 0 ? (
              <div className="bg-brand-surface border border-brand-border p-8 text-center text-white/30 text-sm">
                HLV hiện chưa đăng bất kỳ gói tập nào.
              </div>
            ) : (
              packages.map((pkg) => (
                <div key={pkg.id} className="card-dark border border-brand-border flex flex-col gap-4 p-6 hover:border-brand-red transition-all duration-300">
                  <div>
                    <Badge variant="red" className="mb-2">{pkg.sessionCount} Buổi Tập</Badge>
                    <h4 className="font-semibold text-white text-lg mb-1">{pkg.name}</h4>
                    <p className="text-white/40 text-xs min-h-[40px] mt-1 leading-relaxed">
                      {pkg.description || 'Gói tập luyện thể hình trực tuyến 1-kèm-1 cùng huấn luyện viên LockedIn.'}
                    </p>
                  </div>
                  
                  <div className="pt-4 border-t border-brand-border flex items-center justify-between">
                    <div>
                      <p className="text-white/30 text-[10px] uppercase tracking-wider">Tổng giá trị</p>
                      <p className="text-white font-bold text-xl">{pkg.price.toLocaleString('vi-VN')}đ</p>
                    </div>
                    
                    <button
                      onClick={() => handleSelectPackage(pkg)}
                      disabled={bookingLoading === pkg.id}
                      className="btn-primary text-xs py-2.5 px-4 cursor-pointer flex items-center gap-1.5"
                    >
                      {bookingLoading === pkg.id ? (
                        <>
                          <Loader2 className="animate-spin" size={12} />
                          Đang tạo...
                        </>
                      ) : (
                        <>
                          Đăng Ký
                        </>
                      )}
                    </button>
                  </div>
                </div>
              ))
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default PTDetail;
