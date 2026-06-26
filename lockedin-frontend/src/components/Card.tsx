// src/components/Card.tsx
import React from 'react';

interface CardProps {
  children: React.ReactNode;
  className?: string;
  hover?: boolean;
  onClick?: () => void;
}

const Card: React.FC<CardProps> = ({ children, className = '', hover = true, onClick }) => {
  return (
    <div
      className={`bg-brand-surface border border-brand-border ${
        hover ? 'transition-all duration-300 hover:border-brand-red cursor-pointer' : ''
      } ${className}`}
      onClick={onClick}
    >
      {children}
    </div>
  );
};

export default Card;
