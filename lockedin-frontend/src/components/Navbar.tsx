import React, { useState, useEffect } from 'react';
import { Link, useLocation, useNavigate } from 'react-router-dom';
import { Menu, X, LogOut, User, Bell } from 'lucide-react';
import { useAuth } from '../context/AuthContext';
import { useData } from '../context/DataContext';
import logoImg from '../assets/logo.png';

const Navbar: React.FC = () => {
  const [isScrolled, setIsScrolled] = useState(false);
  const [lastScrollY, setLastScrollY] = useState(0);
  const [isVisible, setIsVisible] = useState(true);
  const [mobileOpen, setMobileOpen] = useState(false);
  const { currentUser, currentRole, logout } = useAuth();
  const { notifications, markNotificationsAsRead, markNotificationAsRead } = useData();
  const [showNotifications, setShowNotifications] = useState(false);
  const unreadCount = notifications ? notifications.filter(n => !n.read).length : 0;
  const location = useLocation();
  const navigate = useNavigate();

  useEffect(() => {
    const handleScroll = () => {
      const currentY = window.scrollY;
      setIsScrolled(currentY > 10);
      // Hide on scroll down, show on scroll up
      if (currentY > lastScrollY && currentY > 80) {
        setIsVisible(false);
      } else {
        setIsVisible(true);
      }
      setLastScrollY(currentY);
    };
    window.addEventListener('scroll', handleScroll, { passive: true });
    return () => window.removeEventListener('scroll', handleScroll);
  }, [lastScrollY]);

  useEffect(() => {
    setMobileOpen(false);
  }, [location.pathname]);

  const handleNotificationClick = (notif: AppNotification) => {
    if (!notif.read) {
      markNotificationAsRead(notif.id);
    }
  };

  const getNavLinks = () => {
    if (!currentUser) {
      return [
        { label: 'Trang Chủ', path: '/' },
        { label: 'Tìm PT', path: '/marketplace' },
      ];
    }
    if (currentRole === 'customer') {
      return [
        { label: 'Trang Chủ', path: '/' },
        { label: 'Tìm PT', path: '/marketplace' },
        { label: 'Lịch Đặt', path: '/customer/bookings' },
        { label: 'Trò Chuyện', path: '/customer/workspace' },
        { label: 'Hồ Sơ', path: '/customer/profile' },
      ];
    }
    if (currentRole === 'pt') {
      return [
        { label: 'Trang Chủ', path: '/' },
        { label: 'Gói Dịch Vụ', path: '/pt/packages' },
        { label: 'Lịch Đặt', path: '/pt/bookings' },
        { label: 'Trò Chuyện', path: '/pt/workspace' },
        { label: 'Hồ Sơ', path: '/pt/profile' },
      ];
    }
    if (currentRole === 'admin') {
      return [
        { label: 'Trang Chủ', path: '/' },
        { label: 'Dashboard', path: '/admin/dashboard' },
      ];
    }
    return [];
  };

  const navLinks = getNavLinks();
  const isActive = (locationPath: string) => location.pathname === locationPath;

  return (
    <>
      <header
        className={`fixed top-0 left-0 right-0 z-50 transition-all duration-300 ${
          isVisible ? 'translate-y-0' : '-translate-y-full'
        } ${isScrolled ? 'bg-black border-b border-brand-border' : 'bg-transparent'}`}
        style={{ paddingTop: 'env(safe-area-inset-top, 0px)' }}
      >
        <div className="section-container">
          <div className="flex items-center justify-between h-16">
            {/* Left: Logo */}
            <div className="flex-1 flex justify-start">
              <Link to="/" className="flex items-center group">
                <img src={logoImg} alt="LockedIn Logo" className="h-12 w-auto object-contain transition-all duration-200 group-hover:scale-105" />
              </Link>
            </div>

            {/* Desktop Nav Links */}
            <nav className="hidden md:flex items-center gap-8">
              {navLinks.map((link) => (
                <Link
                  key={link.path}
                  to={link.path}
                  className={isActive(link.path) ? 'nav-link-active' : 'nav-link'}
                >
                  {link.label}
                </Link>
              ))}
            </nav>

            {/* Desktop Right Actions */}
            <div className="hidden md:flex flex-1 justify-end items-center gap-4">
              {currentUser ? (
                <div className="flex items-center gap-4">
                  {/* Notifications Bell */}
                  <div className="relative">
                    <button
                      onClick={() => setShowNotifications(!showNotifications)}
                      className="relative p-2 text-white/60 hover:text-white cursor-pointer transition-colors"
                      title="Thông báo"
                    >
                      <Bell size={18} />
                      {unreadCount > 0 && (
                        <span className="absolute top-1 right-1 w-4 h-4 bg-brand-red text-white text-[9px] font-bold rounded-full flex items-center justify-center">
                          {unreadCount}
                        </span>
                      )}
                    </button>

                    {/* Notifications dropdown popover */}
                    {showNotifications && (
                      <div className="absolute right-0 mt-2 w-80 bg-brand-dark border border-brand-border shadow-xl z-50 animate-fade-in max-h-96 flex flex-col">
                        <div className="p-4 border-b border-brand-border flex items-center justify-between">
                          <span className="font-montserrat font-bold text-xs uppercase tracking-wider text-white">Thông Báo</span>
                          {unreadCount > 0 && (
                            <button
                              onClick={async () => {
                                try {
                                  await markNotificationsAsRead();
                                } catch (e) {
                                  console.error(e);
                                }
                              }}
                              className="text-[10px] text-brand-red hover:underline uppercase tracking-wider font-semibold cursor-pointer"
                            >
                              Đọc tất cả
                            </button>
                          )}
                        </div>

                        <div className="overflow-y-auto flex-1 divide-y divide-brand-border max-h-64">
                          {(!notifications || notifications.length === 0) ? (
                            <div className="p-6 text-center text-white/30 text-xs">
                              Không có thông báo nào.
                            </div>
                          ) : (
                            notifications.map((notif) => (
                              <div
                                key={notif.id}
                                onClick={() => handleNotificationClick(notif)}
                                className={`p-4 flex flex-col gap-1 cursor-pointer transition-colors ${
                                  !notif.read ? 'bg-brand-red/5' : 'hover:bg-white/[0.02]'
                                }`}
                              >
                                <div className="flex items-center justify-between">
                                  <span className={`text-xs font-semibold ${!notif.read ? 'text-white' : 'text-white/60'}`}>
                                    {notif.title}
                                  </span>
                                  {!notif.read && (
                                    <div className="w-1.5 h-1.5 bg-brand-red rounded-full" />
                                  )}
                                </div>
                                <p className="text-white/40 text-[11px] leading-relaxed">
                                  {notif.message}
                                </p>
                                <span className="text-white/20 text-[9px] mt-1">
                                  {new Date(notif.createdAt).toLocaleDateString('vi-VN')} {new Date(notif.createdAt).toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' })}
                                </span>
                              </div>
                            ))
                          )}
                        </div>
                      </div>
                    )}
                  </div>

                  <Link
                    to={currentRole === 'pt' ? '/pt/profile' : currentRole === 'customer' ? '/customer/profile' : '/'}
                    className="flex items-center gap-2 text-xs text-white/50 hover:text-white uppercase tracking-wider transition-colors duration-200 cursor-pointer"
                  >
                    <User size={14} />
                    <span>{currentRole}</span>
                  </Link>
                  <button
                    onClick={() => { logout(); navigate('/'); }}
                    className="flex items-center gap-2 text-xs text-white/60 hover:text-brand-red uppercase tracking-widest transition-colors duration-200 cursor-pointer"
                  >
                    <LogOut size={14} />
                    <span>Đăng Xuất</span>
                  </button>
                </div>
              ) : (
                <>
                  <Link to="/login" className="nav-link">Đăng Nhập</Link>
                  <Link to="/marketplace" className="btn-primary text-xs py-2 px-5">
                    Tìm PT Ngay
                  </Link>
                </>
              )}
            </div>

            {/* Mobile Actions with Notifications Bell */}
            <div className="flex flex-1 justify-end md:hidden items-center gap-2">
              {currentUser && (
                <div className="relative">
                  <button
                    onClick={() => setShowNotifications(!showNotifications)}
                    className="relative p-2 text-white/60 hover:text-white cursor-pointer"
                  >
                    <Bell size={18} />
                    {unreadCount > 0 && (
                      <span className="absolute top-1 right-1 w-4 h-4 bg-brand-red text-white text-[9px] font-bold rounded-full flex items-center justify-center">
                        {unreadCount}
                      </span>
                    )}
                  </button>
                  {showNotifications && (
                    <div className="absolute right-[-40px] mt-2 w-72 bg-brand-dark border border-brand-border shadow-xl z-50 max-h-80 flex flex-col">
                      <div className="p-3 border-b border-brand-border flex items-center justify-between">
                        <span className="font-montserrat font-bold text-[10px] uppercase tracking-wider text-white">Thông Báo</span>
                        {unreadCount > 0 && (
                          <button
                            onClick={async () => {
                              try {
                                await markNotificationsAsRead();
                              } catch (e) {
                                console.error(e);
                              }
                            }}
                            className="text-[9px] text-brand-red hover:underline uppercase tracking-wider font-semibold cursor-pointer"
                          >
                            Đọc tất cả
                          </button>
                        )}
                      </div>
                      <div className="overflow-y-auto flex-1 divide-y divide-brand-border max-h-60">
                        {(!notifications || notifications.length === 0) ? (
                          <div className="p-4 text-center text-white/30 text-[10px]">
                            Không có thông báo nào.
                          </div>
                        ) : (
                          notifications.map((notif) => (
                            <div
                              key={notif.id}
                              onClick={() => handleNotificationClick(notif)}
                              className={`p-3 flex flex-col gap-1 cursor-pointer transition-colors ${
                                !notif.read ? 'bg-brand-red/5' : 'hover:bg-white/[0.02]'
                              }`}
                            >
                              <div className="flex items-center justify-between">
                                <span className={`text-[11px] font-semibold ${!notif.read ? 'text-white' : 'text-white/60'}`}>
                                  {notif.title}
                                </span>
                                {!notif.read && (
                                  <div className="w-1.5 h-1.5 bg-brand-red rounded-full" />
                                )}
                              </div>
                              <p className="text-white/40 text-[10px] leading-relaxed">
                                {notif.message}
                              </p>
                            </div>
                          ))
                        )}
                      </div>
                    </div>
                  )}
                </div>
              )}

              <button
                className="text-white hover:text-brand-red transition-colors duration-200 cursor-pointer p-2"
                onClick={() => setMobileOpen(!mobileOpen)}
                aria-label="Toggle menu"
              >
                {mobileOpen ? <X size={24} /> : <Menu size={24} />}
              </button>
            </div>
          </div>
        </div>
      </header>

      {/* Mobile Full-Screen Menu */}
      {mobileOpen && (
        <div className="fixed inset-0 z-40 bg-black flex flex-col pt-16 animate-fade-in">
          <nav className="flex flex-col flex-1 section-container py-8 gap-1">
            {navLinks.map((link) => (
              <Link
                key={link.path}
                to={link.path}
                className={`text-3xl font-display uppercase tracking-widest py-4 border-b border-brand-border transition-colors duration-200 ${
                  isActive(link.path) ? 'text-brand-red' : 'text-white hover:text-brand-red'
                }`}
              >
                {link.label}
              </Link>
            ))}

            <div className="mt-auto pt-8 flex flex-col gap-3">
              {currentUser ? (
                <button
                  onClick={() => { logout(); navigate('/'); setMobileOpen(false); }}
                  className="btn-secondary w-full"
                >
                  <LogOut size={16} />
                  Đăng Xuất
                </button>
              ) : (
                <>
                  <Link to="/login" className="btn-secondary w-full text-center">Đăng Nhập</Link>
                  <Link to="/marketplace" className="btn-primary w-full text-center">Tìm PT Ngay</Link>
                </>
              )}
            </div>
          </nav>
        </div>
      )}
    </>
  );
};

export default Navbar;
