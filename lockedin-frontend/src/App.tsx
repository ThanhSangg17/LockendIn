// src/App.tsx
import React from 'react';
import { HashRouter as Router, Routes, Route, Navigate, useLocation } from 'react-router-dom';
import { AuthProvider, useAuth } from './context/AuthContext';
import { DataProvider } from './context/DataContext';

// Core UI Design Elements
import Navbar from './components/Navbar';
import RoleSwitcherHUD from './components/RoleSwitcherHUD';

// Marketing & Auth views
import Home from './pages/Home';
import Login from './pages/Auth/Login';
import Register from './pages/Auth/Register';
import Onboarding from './pages/Auth/Onboarding';

import Marketplace from './pages/Marketplace';
import PTDetail from './pages/PTDetail';
import CheckoutSim from './pages/CheckoutSim';
import WorkspacePage from './pages/WorkspacePage';
import ProfilePage from './pages/ProfilePage';

// PT Portal Spec
import PTPackages from './pages/PT/PTPackages';
import PTBookings from './pages/PT/PTBookings';
import PTPendingVerification from './pages/PT/PTPendingVerification';

// Customer Portal Spec
import CustomerBookings from './pages/Customer/CustomerBookings';

// Admin Portal Spec
import AdminDashboard from './pages/AdminDashboard';

// Route Guard to verify role privileges
const RoleProtectedRoute: React.FC<{ 
  allowedRoles: ('customer' | 'pt' | 'admin')[];
  children: React.ReactNode;
}> = ({ allowedRoles, children }) => {
  const { currentUser, currentRole } = useAuth();
  const location = useLocation();

  if (!currentUser) {
    return <Navigate to="/login" replace />;
  }

  if (currentRole && !allowedRoles.includes(currentRole)) {
    return <Navigate to="/marketplace" replace />;
  }

  if (currentRole === 'pt') {
    const isPendingPage = location.pathname === '/pt/pending';
    if (currentUser.verificationStatus !== 'verified' && !isPendingPage) {
      return <Navigate to="/pt/pending" replace />;
    }
    if (currentUser.verificationStatus === 'verified' && isPendingPage) {
      return <Navigate to="/pt/bookings" replace />;
    }
  }

  return <>{children}</>;
};

const AppContent: React.FC = () => {
  const location = useLocation();
  const isAuthOrOnboarding = ['/login', '/register', '/onboarding'].includes(location.pathname);

  return (
    <>
      {/* Sticky Header Nav */}
      {!isAuthOrOnboarding && <Navbar />}

      {/* Main Page Layout Container */}
      <div 
        className="flex-1 flex flex-col" 
        style={{ 
          paddingTop: isAuthOrOnboarding 
            ? '0px' 
            : 'calc(70px + env(safe-area-inset-top, 0px))' 
        }}
      >
        <Routes>
          {/* Public Views */}
          <Route path="/" element={<Home />} />
          <Route path="/login" element={<Login />} />
          <Route path="/register" element={<Register />} />
          <Route path="/onboarding" element={
            <RoleProtectedRoute allowedRoles={['customer']}>
              <Onboarding />
            </RoleProtectedRoute>
          } />
          <Route path="/marketplace" element={<Marketplace />} />
          <Route path="/marketplace/pt/:ptId" element={<PTDetail />} />
          <Route path="/checkout/:bookingId" element={<CheckoutSim />} />

          {/* Customer Route Guard Board */}
          <Route 
            path="/customer/bookings" 
            element={
              <RoleProtectedRoute allowedRoles={['customer']}>
                <CustomerBookings />
              </RoleProtectedRoute>
            } 
          />
          <Route 
            path="/customer/workspace" 
            element={
              <RoleProtectedRoute allowedRoles={['customer']}>
                <WorkspacePage />
              </RoleProtectedRoute>
            } 
          />
          <Route 
            path="/customer/profile" 
            element={
              <RoleProtectedRoute allowedRoles={['customer']}>
                <ProfilePage />
              </RoleProtectedRoute>
            } 
          />

          {/* PT Route Guard Board */}
          <Route 
            path="/pt/packages" 
            element={
              <RoleProtectedRoute allowedRoles={['pt']}>
                <PTPackages />
              </RoleProtectedRoute>
            } 
          />
          <Route 
            path="/pt/bookings" 
            element={
              <RoleProtectedRoute allowedRoles={['pt']}>
                <PTBookings />
              </RoleProtectedRoute>
            } 
          />
          <Route 
            path="/pt/workspace" 
            element={
              <RoleProtectedRoute allowedRoles={['pt']}>
                <WorkspacePage />
              </RoleProtectedRoute>
            } 
          />
          <Route 
            path="/pt/profile" 
            element={
              <RoleProtectedRoute allowedRoles={['pt']}>
                <ProfilePage />
              </RoleProtectedRoute>
            } 
          />
          <Route 
            path="/pt/pending" 
            element={
              <RoleProtectedRoute allowedRoles={['pt']}>
                <PTPendingVerification />
              </RoleProtectedRoute>
            } 
          />

          {/* Admin Route Guard Board */}
          <Route 
            path="/admin/dashboard" 
            element={
              <RoleProtectedRoute allowedRoles={['admin']}>
                <AdminDashboard />
              </RoleProtectedRoute>
            } 
          />

          {/* Universal Fallback redirect */}
          <Route path="*" element={<Navigate to="/" replace />} />
        </Routes>
      </div>

      {/* Developer Persona HUD Swapping HUD */}
      <RoleSwitcherHUD />
    </>
  );
};

export const App: React.FC = () => {
  return (
    <Router>
      <AuthProvider>
        <DataProvider>
          <AppContent />
        </DataProvider>
      </AuthProvider>
    </Router>
  );
};

export default App;
