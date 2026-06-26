import React, { useEffect, useState } from 'react';
import { useAuth } from '../../context/AuthContext';
import { useNavigate } from 'react-router-dom';
import { Clock, XCircle, AlertCircle, RefreshCw } from 'lucide-react';
import api from '../../services/api';

const PTPendingVerification: React.FC = () => {
  const { currentUser, updateProfile } = useAuth();
  const navigate = useNavigate();
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    // Poll every 30 seconds
    const interval = setInterval(async () => {
      try {
        const profileRes = await api.get('/pts/me/profile');
        if (profileRes.data && profileRes.data.success && profileRes.data.data) {
          const p = profileRes.data.data;
          const status = p.verificationStatus === 3 ? 'verified' : p.verificationStatus === 4 ? 'rejected' : 'pending';
          
          if (status !== currentUser?.verificationStatus) {
            await updateProfile({ verificationStatus: status });
            if (status === 'verified') {
              navigate('/pt/bookings', { replace: true });
            }
          }
        }
      } catch (e) {
        console.warn('Polling profile failed', e);
      }
    }, 30000);

    return () => clearInterval(interval);
  }, [currentUser?.verificationStatus, updateProfile, navigate]);

  const handleReupload = () => {
    // Direct them to profile to re-upload documents
    navigate('/pt/profile');
  };

  const handleManualCheck = async () => {
    setLoading(true);
    try {
      // 1. Check real backend API
      const profileRes = await api.get('/pts/me/profile');
      if (profileRes.data && profileRes.data.success && profileRes.data.data) {
        const p = profileRes.data.data;
        let status = p.verificationStatus === 3 ? 'verified' : p.verificationStatus === 4 ? 'rejected' : 'pending';
        
        // 2. Mock fallback for local testing
        if (currentUser?.email === 'pttest@gmail.com' && status === 'pending') {
           const mockStatus = localStorage.getItem('mock_pt_status');
           if (mockStatus === 'verified' || mockStatus === 'rejected') {
               status = mockStatus;
           }
        }

        await updateProfile({ verificationStatus: status as any });
        if (status === 'verified') {
          navigate('/pt/bookings', { replace: true });
        } else if (status === 'pending') {
          alert('Hồ sơ của bạn vẫn đang được xem xét. Vui lòng quay lại sau.');
        }
      }
    } catch (e) {
      console.warn('Manual check failed', e);
    } finally {
      setLoading(false);
    }
  };

  if (!currentUser) return null;

  return (
    <div className="min-h-screen bg-brand-black flex items-center justify-center p-6">
      <div className="max-w-xl w-full bg-brand-surface border border-brand-border p-10 text-center">
        {currentUser.verificationStatus === 'pending' ? (
          <>
            <div className="w-20 h-20 bg-yellow-500/10 rounded-full flex items-center justify-center mx-auto mb-6">
              <Clock className="w-10 h-10 text-yellow-500" />
            </div>
            <h1 className="text-2xl font-bold text-white mb-4">Hồ sơ đang được xem xét</h1>
            <p className="text-white/60 mb-8 leading-relaxed">
              Cảm ơn bạn đã đăng ký trở thành Huấn Luyện Viên trên LockedIn. Đội ngũ quản trị viên đang kiểm tra thông tin và chứng chỉ của bạn. Quá trình này thường mất từ 1-2 ngày làm việc.
            </p>
            <div className="bg-brand-dark border border-brand-border p-4 flex items-center gap-4 text-left">
              <AlertCircle className="w-6 h-6 text-brand-red flex-shrink-0" />
              <p className="text-sm text-white/70">
                Chúng tôi sẽ thông báo cho bạn ngay khi hồ sơ được duyệt. Trang này sẽ tự động cập nhật.
              </p>
            </div>
          </>
        ) : currentUser.verificationStatus === 'rejected' ? (
          <>
            <div className="w-20 h-20 bg-brand-red/10 rounded-full flex items-center justify-center mx-auto mb-6">
              <XCircle className="w-10 h-10 text-brand-red" />
            </div>
            <h1 className="text-2xl font-bold text-white mb-4">Hồ sơ bị từ chối</h1>
            <p className="text-white/60 mb-8 leading-relaxed">
              Rất tiếc, hồ sơ của bạn chưa đủ điều kiện hoặc thiếu một số giấy tờ quan trọng. Vui lòng kiểm tra lại chứng chỉ và thông tin định danh cá nhân.
            </p>
            <button 
              onClick={handleReupload}
              className="bg-brand-red text-white font-bold py-4 px-8 w-full hover:bg-brand-red/90 transition-colors rounded-xl"
            >
              Cập nhật lại hồ sơ
            </button>
          </>
        ) : null}

        <div className="mt-8">
          <button 
            onClick={handleManualCheck} 
            disabled={loading}
            className="text-white/40 hover:text-white flex items-center justify-center gap-2 mx-auto text-sm transition-colors"
          >
            <RefreshCw className={`w-4 h-4 ${loading ? 'animate-spin' : ''}`} />
            Kiểm tra trạng thái ngay
          </button>
        </div>
      </div>
    </div>
  );
};

export default PTPendingVerification;
