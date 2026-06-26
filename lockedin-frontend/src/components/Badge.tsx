// src/components/Badge.tsx
import React from 'react';

interface BadgeProps {
  variant?: 'red' | 'white' | 'gray' | 'outline' | 'yellow' | 'green';
  children: React.ReactNode;
  className?: string;
}

const Badge: React.FC<BadgeProps> = ({ variant = 'gray', children, className = '' }) => {
  const variants = {
    red: 'badge-red',
    white: 'badge-white',
    yellow: 'badge-yellow',
    green: 'badge-green',
    gray: 'badge-gray',
    outline: 'inline-flex items-center px-2.5 py-0.5 text-xs font-semibold uppercase tracking-wider border border-brand-red text-brand-red',
  };

  return (
    <span className={`${variants[variant]} ${className}`}>
      {children}
    </span>
  );
};

export default Badge;
