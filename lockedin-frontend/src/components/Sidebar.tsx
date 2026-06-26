// src/components/Sidebar.tsx
import React from 'react';
import { Link, useLocation } from 'react-router-dom';
import type { LucideIcon } from 'lucide-react';

interface SidebarLink {
  label: string;
  path: string;
  icon: LucideIcon;
}

interface SidebarProps {
  links: SidebarLink[];
  title?: string;
}

const Sidebar: React.FC<SidebarProps> = ({ links, title }) => {
  const location = useLocation();
  const isActive = (path: string) => location.pathname === path;

  return (
    <aside className="w-64 flex-shrink-0 bg-brand-dark border-r border-brand-border min-h-full hidden md:flex flex-col">
      {title && (
        <div className="px-6 py-5 border-b border-brand-border">
          <p className="section-label">{title}</p>
        </div>
      )}
      <nav className="flex flex-col py-2">
        {links.map((link) => {
          const Icon = link.icon;
          return (
            <Link
              key={link.path}
              to={link.path}
              className={isActive(link.path) ? 'sidebar-link-active' : 'sidebar-link'}
            >
              <Icon size={16} />
              <span>{link.label}</span>
            </Link>
          );
        })}
      </nav>
    </aside>
  );
};

export default Sidebar;
