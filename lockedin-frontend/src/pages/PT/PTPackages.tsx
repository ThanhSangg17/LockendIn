// src/pages/PT/PTPackages.tsx
import React, { useState } from 'react';
import { Plus, Edit3, Eye, EyeOff, CheckCircle, Package, DollarSign, TrendingUp, Loader2 } from 'lucide-react';
import Modal from '../../components/Modal';
import Badge from '../../components/Badge';
import { useData } from '../../context/DataContext';
const PTPackages: React.FC = () => {
  const { packages, bookings, createPackage, updatePackage } = useData();

  const [showModal, setShowModal] = useState(false);
  const [editing, setEditing] = useState<any | null>(null);
  const [form, setForm] = useState({ name: '', desc: '', sessions: 8, price: 2000000, active: true });
  const [saving, setSaving] = useState(false);

  const openAdd = () => {
    setEditing(null);
    setForm({ name: '', desc: '', sessions: 8, price: 2000000, active: true });
    setShowModal(true);
  };

  const openEdit = (pkg: any) => {
    setEditing(pkg);
    setForm({ 
      name: pkg.name, 
      desc: pkg.description || '', 
      sessions: pkg.sessionsCount, 
      price: pkg.price, 
      active: pkg.isActive 
    });
    setShowModal(true);
  };

  const handleSave = async () => {
    setSaving(true);
    try {
      if (editing) {
        await updatePackage(editing.id, form.name, form.desc, form.sessions, form.price, form.active);
      } else {
        await createPackage(form.name, form.desc, form.sessions, form.price);
      }
      setShowModal(false);
    } catch (e) {
      console.error('Failed to save package:', e);
      alert('Không thể lưu gói tập.');
    } finally {
      setSaving(false);
    }
  };

  const toggleActive = async (pkg: any) => {
    try {
      await updatePackage(pkg.id, pkg.name, pkg.description || '', pkg.sessionsCount, pkg.price, !pkg.isActive);
    } catch (e) {
      console.error('Failed to toggle active status:', e);
    }
  };

  // Calculate live statistics
  const totalRevenue = bookings
    .filter((b) => b.status === 'Active' || b.status === 'Completed')
    .reduce((sum, b) => sum + b.price, 0);
  const totalBookings = bookings.length;

  return (
    <div className="min-h-screen bg-brand-black mobile-content-pad pb-12">
      {/* Header */}
      <div className="border-b border-brand-border bg-brand-dark">
        <div className="section-container py-10">
          <p className="section-label mb-2">Huấn Luyện Viên</p>
          <div className="flex items-center justify-between">
            <h1 className="page-title">Gói Dịch Vụ</h1>
            <button onClick={openAdd} className="btn-primary text-xs py-3 cursor-pointer">
              <Plus size={14} />
              Thêm Gói Mới
            </button>
          </div>
        </div>
      </div>

      <div className="section-container py-8">
        {/* Stats */}
        <div className="grid grid-cols-3 gap-4 mb-8">
          {[
            { icon: Package, label: 'Tổng Gói', value: packages.length.toString() },
            { icon: DollarSign, label: 'Doanh Thu', value: `${(totalRevenue / 1000000).toFixed(2)}M` },
            { icon: TrendingUp, label: 'Tổng Đặt', value: totalBookings.toString() },
          ].map((s, i) => {
            const Icon = s.icon;
            return (
              <div key={i} className="bg-brand-surface border border-brand-border p-6 flex items-center gap-4">
                <div className="w-10 h-10 bg-brand-dark border border-brand-border flex items-center justify-center flex-shrink-0">
                  <Icon size={18} className="text-white/40" />
                </div>
                <div>
                  <p className="font-mono font-semibold text-3xl text-white">{s.value}</p>
                  <p className="text-white/30 text-xs uppercase tracking-widest">{s.label}</p>
                </div>
              </div>
            );
          })}
        </div>

        {/* Packages Grid */}
        <div className="grid md:grid-cols-3 gap-6">
          {packages.map((pkg) => (
            <div key={pkg.id} className={`bg-brand-surface border flex flex-col transition-all duration-300 ${pkg.isActive ? 'border-brand-border hover:border-brand-red' : 'border-brand-border opacity-60'}`}>
              {/* Top accent */}
              <div className={`h-0.5 w-full ${pkg.isActive ? 'bg-brand-red' : 'bg-brand-muted'}`} />

              <div className="p-6 flex-1">
                <div className="flex items-start justify-between mb-4">
                  <h3 className="font-montserrat font-extrabold text-xl text-white uppercase tracking-wider truncate max-w-[70%]" title={pkg.name}>
                    {pkg.name}
                  </h3>
                  <Badge variant={pkg.isActive ? 'red' : 'gray'}>{pkg.isActive ? 'Đang Bán' : 'Ẩn'}</Badge>
                </div>

                <p className="text-white/40 text-sm leading-relaxed mb-6 min-h-[48px]">
                  {pkg.description || 'Gói huấn luyện thể hình trực tuyến 1-kèm-1 cùng huấn luyện viên.'}
                </p>

                <div className="grid grid-cols-2 gap-3 mb-6">
                  {[
                    { label: 'Số Buổi', value: `${pkg.sessionsCount} buổi` },
                    { label: 'Đã Đặt', value: `${bookings.filter(b => b.packageId === pkg.id).length} lần` },
                    { label: 'Giá / Gói', value: `${(pkg.price / 1000000).toFixed(1)}M` },
                    { label: 'Đánh Giá', value: '5.0 ⭐' },
                  ].map((s, i) => (
                    <div key={i} className="bg-brand-dark p-3 rounded-2xl">
                      <p className="text-white/30 text-[10px] uppercase tracking-wider mb-1">{s.label}</p>
                      <p className="text-white font-semibold text-sm">{s.value}</p>
                    </div>
                  ))}
                </div>

                <div className="text-center border border-brand-border p-3 mb-4">
                  <p className="text-white/30 text-xs uppercase tracking-wider mb-1">Giá Niêm Yết</p>
                  <p className="font-mono font-semibold text-3xl text-white">{pkg.price.toLocaleString('vi-VN')}đ</p>
                </div>
              </div>

              {/* Actions */}
              <div className="flex border-t border-brand-border">
                <button
                  onClick={() => openEdit(pkg)}
                  className="flex-1 flex items-center justify-center gap-2 py-3.5 text-white/50 hover:text-white hover:bg-white/5 text-xs uppercase tracking-widest transition-all duration-200 cursor-pointer"
                >
                  <Edit3 size={12} />
                  Chỉnh Sửa
                </button>
                <div className="w-px bg-brand-border" />
                <button
                  onClick={() => toggleActive(pkg)}
                  className="flex-1 flex items-center justify-center gap-2 py-3.5 text-white/50 hover:text-white hover:bg-white/5 text-xs uppercase tracking-widest transition-all duration-200 cursor-pointer"
                >
                  {pkg.isActive ? <EyeOff size={12} /> : <Eye size={12} />}
                  {pkg.isActive ? 'Ẩn Gói' : 'Hiện Gói'}
                </button>
              </div>
            </div>
          ))}

          {/* Add new card */}
          <button
            onClick={openAdd}
            className="border border-dashed border-brand-border min-h-[250px] flex flex-col items-center justify-center gap-3 text-white/20 hover:border-brand-red hover:text-white/60 transition-all duration-300 cursor-pointer p-8"
          >
            <Plus size={32} />
            <span className="font-display text-xl uppercase tracking-wider">Thêm Gói Mới</span>
          </button>
        </div>
      </div>

      {/* Modal */}
      <Modal isOpen={showModal} onClose={() => setShowModal(false)} title={editing ? 'Chỉnh Sửa Gói' : 'Thêm Gói Mới'}>
        <div className="flex flex-col gap-5">
          <div>
            <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Tên Gói</label>
            <input 
              type="text" 
              value={form.name} 
              onChange={(e) => setForm({ ...form, name: e.target.value })} 
              className="input-dark" 
              placeholder="VD: Gói Tiêu Chuẩn" 
              disabled={saving}
            />
          </div>
          <div>
            <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Mô Tả</label>
            <textarea 
              value={form.desc} 
              onChange={(e) => setForm({ ...form, desc: e.target.value })} 
              rows={3} 
              className="input-dark resize-none" 
              placeholder="Mô tả gói tập..." 
              disabled={saving}
            />
          </div>
          <div className="grid grid-cols-2 gap-4">
            <div>
              <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Số Buổi</label>
              <input 
                type="number" 
                value={form.sessions} 
                onChange={(e) => setForm({ ...form, sessions: Number(e.target.value) })} 
                className="input-dark" 
                min={1} 
                disabled={saving}
              />
            </div>
            <div>
              <label className="block text-white/40 text-xs uppercase tracking-widest mb-2">Giá (đ)</label>
              <input 
                type="number" 
                value={form.price} 
                onChange={(e) => setForm({ ...form, price: Number(e.target.value) })} 
                className="input-dark" 
                step={100000} 
                disabled={saving}
              />
            </div>
          </div>
          <div className="flex items-center gap-3">
            <button
              type="button"
              onClick={() => setForm({ ...form, active: !form.active })}
              className={`w-10 h-5 rounded-full transition-all duration-200 relative cursor-pointer ${form.active ? 'bg-brand-red' : 'bg-brand-muted'}`}
              disabled={saving}
            >
              <div className={`w-4 h-4 bg-white rounded-full absolute top-0.5 transition-all duration-200 ${form.active ? 'left-5' : 'left-0.5'}`} />
            </button>
            <span className="text-white/50 text-sm">{form.active ? 'Hiển thị cho khách hàng' : 'Ẩn gói tập này'}</span>
          </div>
          <div className="flex gap-3 pt-2">
            <button onClick={handleSave} className="btn-primary flex-1 text-xs py-3 justify-center" disabled={saving}>
              {saving ? (
                <Loader2 className="animate-spin" size={14} />
              ) : (
                <>
                  <CheckCircle size={14} />
                  {editing ? 'Lưu Thay Đổi' : 'Tạo Gói'}
                </>
              )}
            </button>
            <button onClick={() => setShowModal(false)} className="btn-secondary flex-1 text-xs py-3" disabled={saving}>
              Hủy
            </button>
          </div>
        </div>
      </Modal>
    </div>
  );
};

export default PTPackages;
