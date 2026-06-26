// src/pages/Auth/Onboarding.tsx
import React, { useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { ArrowRight, ChevronUp, ChevronDown } from 'lucide-react';
import { useAuth } from '../../context/AuthContext';

const Onboarding: React.FC = () => {
  const [step, setStep] = useState(1);
  const { updateProfile } = useAuth();
  const navigate = useNavigate();

  // Onboarding states (Default values matching provided style guides)
  const [gender, setGender] = useState<'male' | 'female'>('male');
  const [age, setAge] = useState(36);
  const [weight, setWeight] = useState(54);
  const [height, setHeight] = useState(167);
  const [goal, setGoal] = useState('Giảm Cân');
  const [activity, setActivity] = useState('Trung Bình');

  // Multi-step definitions
  const totalSteps = 6;

  const handleNext = () => {
    if (step < totalSteps) {
      setStep(step + 1);
    } else {
      // Final step: Save to profile and navigate to workspace
      updateProfile({
        gender,
        height,
        weight,
        fitnessGoal: `${goal} (${activity})`,
        healthNotes: `Tuổi: ${age} | Cấp độ tập luyện: ${activity}`
      });
      navigate('/marketplace');
    }
  };

  const handleBack = () => {
    if (step > 1) {
      setStep(step - 1);
    } else {
      navigate('/register');
    }
  };

  // Age Selector Helpers
  const agesList = Array.from({ length: 70 }, (_, i) => i + 15); // ages 15 to 84
  const ageIndex = agesList.indexOf(age);
  const scrollAges = [
    agesList[ageIndex - 2] || '',
    agesList[ageIndex - 1] || '',
    age,
    agesList[ageIndex + 1] || '',
    agesList[ageIndex + 2] || '',
  ];

  // Height Selector Helpers
  const heightsList = Array.from({ length: 120 }, (_, i) => i + 100); // heights 100cm to 219cm
  const heightIndex = heightsList.indexOf(height);
  const scrollHeights = [
    heightsList[heightIndex - 2] || '',
    heightsList[heightIndex - 1] || '',
    height,
    heightsList[heightIndex + 1] || '',
    heightsList[heightIndex + 2] || '',
  ];

  return (
    <div className="min-h-screen bg-black text-white flex flex-col justify-between p-6 overflow-hidden select-none">
      
      {/* ─── HEADER (Progress bar & Step indicator) ─── */}
      <div className="w-full max-w-md mx-auto pt-6">
        <div className="flex justify-between items-center text-[10px] uppercase tracking-[0.2em] text-white/40 mb-3 font-montserrat font-bold">
          <span>Khảo Sát Thể Trạng</span>
          <span className="text-brand-red">{step} / {totalSteps}</span>
        </div>
        <div className="w-full h-1 bg-brand-border rounded-full overflow-hidden">
          <div 
            className="h-full bg-brand-red transition-all duration-300 rounded-full" 
            style={{ width: `${(step / totalSteps) * 100}%` }}
          />
        </div>
      </div>

      {/* ─── MAIN CONTENT BODY ─── */}
      <div className="flex-1 flex items-center justify-center w-full max-w-md mx-auto my-8">
        
        {/* STEP 1: GENDER */}
        {step === 1 && (
          <div className="w-full text-center flex flex-col items-center animate-fade-in">
            <h2 className="font-montserrat font-black text-3xl uppercase tracking-wider mb-2 text-white">Hãy chia sẻ về bạn!</h2>
            <p className="text-white/40 text-xs tracking-wide max-w-[280px] mb-12">
              Để mang lại trải nghiệm huấn luyện tốt nhất, chúng tôi cần biết giới tính của bạn.
            </p>
            
            <div className="flex flex-col gap-6 w-full items-center">
              {/* Male option */}
              <button 
                onClick={() => setGender('male')}
                className={`w-32 h-32 rounded-full flex flex-col items-center justify-center cursor-pointer transition-all duration-300 ${
                  gender === 'male' 
                    ? 'bg-brand-red scale-110 shadow-[0_0_24px_rgba(230,0,0,0.4)]' 
                    : 'bg-brand-surface border border-brand-border hover:border-white/20'
                }`}
              >
                <span className="text-4xl font-bold font-sans">♂</span>
                <span className="text-[10px] uppercase tracking-widest font-semibold mt-1">Nam</span>
              </button>

              {/* Female option */}
              <button 
                onClick={() => setGender('female')}
                className={`w-32 h-32 rounded-full flex flex-col items-center justify-center cursor-pointer transition-all duration-300 ${
                  gender === 'female' 
                    ? 'bg-brand-red scale-110 shadow-[0_0_24px_rgba(230,0,0,0.4)]' 
                    : 'bg-brand-surface border border-brand-border hover:border-white/20'
                }`}
              >
                <span className="text-4xl font-bold font-sans">♀</span>
                <span className="text-[10px] uppercase tracking-widest font-semibold mt-1">Nữ</span>
              </button>
            </div>
          </div>
        )}

        {/* STEP 2: AGE */}
        {step === 2 && (
          <div className="w-full text-center flex flex-col items-center animate-fade-in">
            <h2 className="font-montserrat font-black text-3xl uppercase tracking-wider mb-2 text-white">Bạn bao nhiêu tuổi?</h2>
            <p className="text-white/40 text-xs tracking-wide max-w-[280px] mb-12">
              Tuổi tác giúp chúng tôi tính toán chính xác tỷ lệ trao đổi chất của bạn.
            </p>

            <div className="relative flex flex-col items-center h-[280px] justify-center w-full max-w-[200px]">
              {/* Selector buttons */}
              <button 
                onClick={() => setAge(Math.max(15, age - 1))}
                className="p-2 text-white/30 hover:text-white cursor-pointer transition-colors"
              >
                <ChevronUp size={24} />
              </button>

              {/* Age Scroll View */}
              <div className="flex flex-col items-center justify-center my-3 relative w-full">
                {/* Horizontal marker lines */}
                <div className="absolute top-[40%] bottom-[40%] left-0 right-0 border-y border-brand-red pointer-events-none" />
                
                {scrollAges.map((val, idx) => {
                  const isCurrent = idx === 2;
                  return (
                    <button
                      key={idx}
                      onClick={() => typeof val === 'number' && setAge(val)}
                      className={`h-12 flex items-center justify-center transition-all duration-200 cursor-pointer ${
                        isCurrent 
                          ? 'text-4xl font-black text-white font-mono scale-110' 
                          : 'text-lg text-white/20 font-mono font-medium'
                      }`}
                    >
                      {val}
                    </button>
                  );
                })}
              </div>

              <button 
                onClick={() => setAge(Math.min(84, age + 1))}
                className="p-2 text-white/30 hover:text-white cursor-pointer transition-colors"
              >
                <ChevronDown size={24} />
              </button>
            </div>
          </div>
        )}

        {/* STEP 3: WEIGHT */}
        {step === 3 && (
          <div className="w-full text-center flex flex-col items-center animate-fade-in">
            <h2 className="font-montserrat font-black text-3xl uppercase tracking-wider mb-2 text-white">Cân nặng của bạn?</h2>
            <p className="text-white/40 text-xs tracking-wide max-w-[280px] mb-12">
              Bạn có thể dễ dàng cập nhật chỉ số cân nặng này sau đó.
            </p>

            {/* Large screen value display */}
            <div className="flex items-baseline gap-1 mb-8">
              <span className="font-montserrat font-black text-6xl text-white font-mono">{weight}</span>
              <span className="text-sm font-semibold uppercase tracking-wider text-white/50">kg</span>
            </div>

            {/* Horizontal visual ruler */}
            <div className="relative w-full max-w-[320px] h-20 flex flex-col justify-end overflow-hidden mb-6">
              {/* SVG sliding ruler notches */}
              <div 
                className="flex items-end justify-center transition-transform duration-300"
                style={{ transform: `translateX(${(65 - weight) * 12}px)` }}
              >
                {Array.from({ length: 121 }).map((_, i) => {
                  const val = i + 30; // 30kg to 150kg
                  const isMajor = val % 5 === 0;
                  const isCurrent = val === weight;
                  return (
                    <div 
                      key={i} 
                      onClick={() => setWeight(val)}
                      className="flex flex-col items-center flex-shrink-0 cursor-pointer"
                      style={{ width: '12px' }}
                    >
                      {isMajor && (
                        <span className={`text-[8px] font-mono font-bold mb-2 transition-colors duration-200 ${isCurrent ? 'text-brand-red' : 'text-white/20'}`}>
                          {val}
                        </span>
                      )}
                      <div 
                        className={`w-0.5 rounded-full transition-all duration-200 ${
                          isCurrent 
                            ? 'h-10 bg-brand-red shadow-[0_0_8px_#E60000]' 
                            : isMajor ? 'h-6 bg-brand-muted' : 'h-4 bg-brand-border'
                        }`} 
                      />
                    </div>
                  );
                })}
              </div>
              
              {/* Vertical red cursor line */}
              <div className="absolute left-1/2 -translate-x-1/2 bottom-0 w-0.5 h-12 bg-brand-red shadow-[0_0_8px_#E60000] pointer-events-none" />
            </div>

            {/* Hidden native slider styled nicely to capture touch drags */}
            <input 
              type="range" 
              min="30" 
              max="150" 
              value={weight} 
              onChange={(e) => setWeight(parseInt(e.target.value))}
              className="w-full max-w-[280px] accent-brand-red cursor-pointer bg-brand-border h-1 rounded-full outline-none"
            />
          </div>
        )}

        {/* STEP 4: HEIGHT */}
        {step === 4 && (
          <div className="w-full text-center flex flex-col items-center animate-fade-in">
            <h2 className="font-montserrat font-black text-3xl uppercase tracking-wider mb-2 text-white">Chiều cao của bạn?</h2>
            <p className="text-white/40 text-xs tracking-wide max-w-[280px] mb-12">
              Chỉ số này giúp chúng tôi phân tích chỉ số thể trọng BMI của bạn.
            </p>

            <div className="relative flex flex-col items-center h-[280px] justify-center w-full max-w-[200px]">
              {/* Selector buttons */}
              <button 
                onClick={() => setHeight(Math.max(100, height - 1))}
                className="p-2 text-white/30 hover:text-white cursor-pointer transition-colors"
              >
                <ChevronUp size={24} />
              </button>

              {/* Height Scroll View */}
              <div className="flex flex-col items-center justify-center my-3 relative w-full">
                {/* Horizontal marker lines */}
                <div className="absolute top-[40%] bottom-[40%] left-0 right-0 border-y border-brand-red pointer-events-none" />
                
                {scrollHeights.map((val, idx) => {
                  const isCurrent = idx === 2;
                  return (
                    <button
                      key={idx}
                      onClick={() => typeof val === 'number' && setHeight(val)}
                      className={`h-12 flex items-center justify-center transition-all duration-200 cursor-pointer ${
                        isCurrent 
                          ? 'text-4xl font-black text-white font-mono scale-110' 
                          : 'text-lg text-white/20 font-mono font-medium'
                      }`}
                    >
                      {val}{isCurrent && <span className="text-xs font-semibold uppercase tracking-wider ml-1 text-white/50">cm</span>}
                    </button>
                  );
                })}
              </div>

              <button 
                onClick={() => setHeight(Math.min(219, height + 1))}
                className="p-2 text-white/30 hover:text-white cursor-pointer transition-colors"
              >
                <ChevronDown size={24} />
              </button>
            </div>
          </div>
        )}

        {/* STEP 5: FITNESS GOAL */}
        {step === 5 && (
          <div className="w-full text-center flex flex-col items-center animate-fade-in">
            <h2 className="font-montserrat font-black text-3xl uppercase tracking-wider mb-2 text-white">Mục tiêu của bạn?</h2>
            <p className="text-white/40 text-xs tracking-wide max-w-[280px] mb-12">
              Chúng tôi sẽ sắp xếp lộ trình phù hợp với mục tiêu bạn chọn.
            </p>

            <div className="flex flex-col gap-3 w-full">
              {[
                { id: 'Giảm Cân', title: 'Giảm Cân', desc: 'Đốt mỡ thừa & săn chắc cơ thể' },
                { id: 'Tăng Cơ & Sức Mạnh', title: 'Tăng Cơ & Sức Mạnh', desc: 'Xây dựng khối cơ & tăng sức chịu đựng' },
                { id: 'Giữ Dáng', title: 'Giữ Dáng', desc: 'Duy trì vóc dáng & cải thiện sức bền' },
                { id: 'Dẻo Dai Hơn', title: 'Dẻo Dai Hơn', desc: 'Cải thiện sự linh hoạt & dẻo dai' },
                { id: 'Học Cơ Bản', title: 'Học Cơ Bản', desc: 'Tiếp cận các bài tập nền tảng ban đầu' },
              ].map((item) => {
                const isSelected = goal === item.id;
                return (
                  <button
                    key={item.id}
                    onClick={() => setGoal(item.id)}
                    className={`w-full p-4 border text-left cursor-pointer transition-all duration-200 flex flex-col gap-0.5 ${
                      isSelected 
                        ? 'border-brand-red bg-brand-red/10 text-white shadow-[0_0_12px_rgba(230,0,0,0.1)]' 
                        : 'border-brand-border hover:border-white/20 bg-brand-surface text-white/50'
                    }`}
                  >
                    <span className={`font-semibold text-sm ${isSelected ? 'text-white' : 'text-white/70'}`}>
                      {item.title}
                    </span>
                    <span className="text-[10px] text-white/30 uppercase tracking-widest">{item.desc}</span>
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {/* STEP 6: ACTIVITY LEVEL */}
        {step === 6 && (
          <div className="w-full text-center flex flex-col items-center animate-fade-in">
            <h2 className="font-montserrat font-black text-3xl uppercase tracking-wider mb-2 text-white">Tần suất vận động?</h2>
            <p className="text-white/40 text-xs tracking-wide max-w-[280px] mb-12">
              Khai báo cấp độ hiện tại để thiết kế bài tập an toàn, phù hợp lực cơ.
            </p>

            <div className="flex flex-col gap-3 w-full">
              {[
                { id: 'Lính Mới', label: 'Lính Mới', desc: 'Ít vận động, ngồi làm việc văn phòng' },
                { id: 'Bắt Đầu', label: 'Người Bắt Đầu', desc: 'Tập luyện nhẹ nhàng 1-2 lần / tuần' },
                { id: 'Trung Bình', label: 'Trung Bình', desc: 'Tập luyện đều đặn 3-4 lần / tuần' },
                { id: 'Nâng Cao', label: 'Nâng Cao', desc: 'Tập luyện cường độ cao 5-6 lần / tuần' },
                { id: 'Quái Vật', label: 'Quái Vật Thể Hình', desc: 'Vận động viên hoặc tập luyện hardcore hàng ngày' },
              ].map((item) => {
                const isSelected = activity === item.id;
                return (
                  <button
                    key={item.id}
                    onClick={() => setActivity(item.id)}
                    className={`w-full p-4 border text-left cursor-pointer transition-all duration-200 flex flex-col gap-0.5 ${
                      isSelected 
                        ? 'border-brand-red bg-brand-red/10 text-white shadow-[0_0_12px_rgba(230,0,0,0.1)]' 
                        : 'border-brand-border hover:border-white/20 bg-brand-surface text-white/50'
                    }`}
                  >
                    <span className={`font-semibold text-sm ${isSelected ? 'text-white' : 'text-white/70'}`}>
                      {item.label}
                    </span>
                    <span className="text-[10px] text-white/30 uppercase tracking-widest">{item.desc}</span>
                  </button>
                );
              })}
            </div>
          </div>
        )}

      </div>

      {/* ─── BOTTOM CONTROLS (Back / Next button group) ─── */}
      <div className="w-full max-w-md mx-auto flex items-center justify-between pb-6 pt-4 border-t border-brand-border">
        {/* Back button */}
        <button 
          onClick={handleBack}
          className="w-12 h-12 rounded-full bg-brand-surface border border-brand-border flex items-center justify-center text-white/50 hover:text-white hover:border-white/20 cursor-pointer active:scale-95 transition-all"
        >
          <span className="text-xl font-bold font-sans">←</span>
        </button>

        {/* Next / Start button */}
        <button 
          onClick={handleNext}
          className="btn-primary flex items-center gap-2 py-3.5 px-8 rounded-full cursor-pointer animate-pulse-red"
        >
          <span>{step === totalSteps ? 'Bắt Đầu' : 'Tiếp Theo'}</span>
          <ArrowRight size={16} />
        </button>
      </div>

    </div>
  );
};

export default Onboarding;
