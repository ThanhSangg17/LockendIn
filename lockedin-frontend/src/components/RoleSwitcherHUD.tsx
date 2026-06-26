// src/components/RoleSwitcherHUD.tsx
import React, { useState } from 'react';
import { ChevronUp, User, Dumbbell, Shield } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useNavigate } from 'react-router-dom';

const roles = [
  { id: 'customer', label: 'Người Dùng', icon: User, route: '/marketplace' },
  { id: 'pt', label: 'HLV', icon: Dumbbell, route: '/pt/packages' },
  { id: 'admin', label: 'Admin', icon: Shield, route: '/admin/dashboard' },
] as const;

const RoleSwitcherHUD: React.FC = () => {
  const [open, setOpen] = useState(false);
  const { currentRole, devSwitchRole } = useAuth();
  const navigate = useNavigate();

  const handleSwitch = (roleId: 'customer' | 'pt' | 'admin', route: string) => {
    devSwitchRole(roleId);
    navigate(route);
    setOpen(false);
  };

  return (
    <div className="fixed bottom-6 right-6 z-50 flex flex-col items-end gap-2">
      {/* Role Options */}
      {open && (
        <div className="flex flex-col gap-1.5 animate-fade-up">
          {roles.map((role) => {
            const Icon = role.icon;
            const isActive = currentRole === role.id;
            return (
              <button
                key={role.id}
                onClick={() => handleSwitch(role.id, role.route)}
                className={`flex items-center gap-2 px-4 py-2.5 text-xs font-semibold uppercase tracking-widest transition-all duration-200 cursor-pointer border ${
                  isActive
                    ? 'bg-brand-red border-brand-red text-white'
                    : 'bg-black border-brand-border text-white/60 hover:border-brand-red hover:text-white'
                }`}
              >
                <Icon size={14} />
                {role.label}
              </button>
            );
          })}
        </div>
      )}

      {/* Toggle Button */}
      <button
        onClick={() => setOpen(!open)}
        className="flex items-center gap-2 bg-brand-red text-white px-4 py-2.5 text-xs font-semibold uppercase tracking-widest cursor-pointer hover:bg-brand-red-dark transition-colors duration-200 border border-brand-red"
      >
        <ChevronUp
          size={14}
          className={`transition-transform duration-300 ${open ? 'rotate-180' : ''}`}
        />
        <span>Dev: {currentRole?.toUpperCase() || 'NONE'}</span>
      </button>
    </div>
  );
};

export default RoleSwitcherHUD;
