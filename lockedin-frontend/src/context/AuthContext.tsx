// src/context/AuthContext.tsx
import React, { createContext, useContext, useState, useEffect } from 'react';
import api from '../services/api';
import { getMockUsers } from '../services/mockAuth';
import type { User, UserRole } from '../services/mockAuth';

interface AuthContextType {
  currentUser: User | null;
  currentRole: UserRole | null;
  users: User[];
  login: (email: string, password?: string) => Promise<{ success: boolean; role?: UserRole }>;
  googleLogin: (idToken: string, role?: UserRole) => Promise<{ success: boolean; role?: UserRole; message?: string }>;
  register: (fullName: string, email: string, role: UserRole, phone: string, password?: string) => Promise<User>;
  logout: () => void;
  updateProfile: (updatedData: Partial<User>) => Promise<void>;
  devSwitchRole: (role: UserRole) => void;
  requestVerification: (personalId: File, certificates: File[], notes?: string) => void;
  refreshUsersList: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

const getInitialSession = () => {
  try {
    const savedSession = localStorage.getItem('lockedin_session');
    if (savedSession) {
      const parsed = JSON.parse(savedSession);
      if (parsed && parsed.accessToken) {
        const mappedRole = parsed.role;
        return {
          user: {
            id: parsed.userId,
            email: mappedRole === 'pt' ? 'pttest@gmail.com' : mappedRole === 'admin' ? 'admin@lockedin.vn' : 'usertest@gmail.com',
            fullName: mappedRole === 'pt' ? 'PT Test Account' : mappedRole === 'admin' ? 'LockedIn Admin' : 'Customer Test Account',
            role: mappedRole,
            isEmailVerified: true
          },
          role: mappedRole
        };
      }
    }
  } catch (e) {
    console.warn('Failed to parse initial session:', e);
  }
  return null;
};

export const AuthProvider: React.FC<{ children: React.ReactNode }> = ({ children }) => {
  const [users, setUsers] = useState<User[]>([]);
  const initialSession = getInitialSession();
  const [currentUser, setCurrentUser] = useState<User | null>(initialSession ? initialSession.user : null);
  const [currentRole, setCurrentRole] = useState<UserRole | null>(initialSession ? initialSession.role : null);

  // Load active session from localStorage on startup & fetch from C# backend
  useEffect(() => {
    const fetchMe = async () => {
      const savedSession = localStorage.getItem('lockedin_session');
      if (savedSession) {
        try {
          const parsedSession = JSON.parse(savedSession) as { userId: string; role: UserRole; accessToken: string };
          if (parsedSession.accessToken) {
            // Set current state from cache initially for fast rendering
            setCurrentRole(parsedSession.role);
            setCurrentUser({
              id: parsedSession.userId,
              email: parsedSession.role === 'pt' ? 'pttest@gmail.com' : parsedSession.role === 'admin' ? 'admin@lockedin.vn' : 'usertest@gmail.com',
              fullName: parsedSession.role === 'pt' ? 'PT Test Account' : parsedSession.role === 'admin' ? 'LockedIn Admin' : 'Customer Test Account',
              role: parsedSession.role,
              isEmailVerified: true
            });

            // Fetch fresh details from real C# Backend
            const response = await api.get('/auth/me');
            const envelope = response.data;
            if (envelope && envelope.success && envelope.data) {
              const data = envelope.data;
              let mappedRole: UserRole = 'customer';
              if (data.role === 2) mappedRole = 'pt';
              else if (data.role === 3) mappedRole = 'admin';

              let phone = '';
              let avatar = '';
              try {
                const userRes = await api.get('/users/me');
                if (userRes.data?.success && userRes.data.data) {
                  phone = userRes.data.data.phone || '';
                  avatar = userRes.data.data.avatarUrl || '';
                }
              } catch (userErr) {
                console.warn('Could not fetch general user me info:', userErr);
              }

              setCurrentUser({
                id: data.userId,
                email: data.email,
                fullName: data.fullName,
                role: mappedRole,
                isEmailVerified: true,
                phoneNumber: phone,
                avatar: avatar
              });
              setCurrentRole(mappedRole);

              // Now fetch extra profile details
              if (mappedRole === 'customer') {
                try {
                  const profileRes = await api.get('/customers/me/profile');
                  if (profileRes.data?.success && profileRes.data.data) {
                    const p = profileRes.data.data;
                    setCurrentUser(prev => prev ? {
                      ...prev,
                      gender: p.gender,
                      height: p.heightCm,
                      weight: p.weightKg,
                      fitnessGoal: p.fitnessGoal,
                      healthNotes: p.healthNote
                    } : null);
                  }
                } catch (err) {}
              } else if (mappedRole === 'pt') {
                try {
                  const profileRes = await api.get('/pts/me/profile');
                  if (profileRes.data?.success && profileRes.data.data) {
                    const p = profileRes.data.data;
                    setCurrentUser(prev => prev ? {
                      ...prev,
                      bio: p.bio,
                      experienceYears: p.experienceYears,
                      specialization: p.specialization ? p.specialization.split(',') : [],
                      verificationStatus: p.verificationStatus === 3 ? 'verified' : p.verificationStatus === 4 ? 'rejected' : 'pending'
                    } : null);
                  }
                } catch (err) {}
              }
            }
          }
        } catch (e) {
          console.warn('Failed to verify session with backend, clearing:', e);
          localStorage.removeItem('lockedin_session');
          setCurrentUser(null);
          setCurrentRole(null);
        }
      }
    };

    fetchMe();

    // Listen to unauthorized event dispatched from axios client
    const handleLogoutEvent = () => {
      logout();
    };
    window.addEventListener('auth_logout', handleLogoutEvent);
    return () => {
      window.removeEventListener('auth_logout', handleLogoutEvent);
    };
  }, []);

  const refreshUsersList = () => {
    const loaded = getMockUsers();
    setUsers(loaded);
  };

  const login = async (email: string, password?: string): Promise<{ success: boolean; role?: UserRole }> => {
    try {
      const response = await api.post('/auth/login', {
        email,
        password: password || 'Password123'
      });
      
      const envelope = response.data;
      if (envelope && envelope.success && envelope.data) {
        const authData = envelope.data;
        let mappedRole: UserRole = 'customer';
        if (authData.role === 2) {
          mappedRole = 'pt';
        } else if (authData.role === 3) {
          mappedRole = 'admin';
        }

        const user: User = {
          id: authData.userId,
          email: authData.email,
          fullName: authData.fullName,
          role: mappedRole,
          isEmailVerified: true,
        };

        setCurrentUser(user);
        setCurrentRole(mappedRole);
        localStorage.setItem(
          'lockedin_session',
          JSON.stringify({
            userId: user.id,
            role: mappedRole,
            accessToken: authData.accessToken,
            refreshToken: authData.refreshToken,
          })
        );

        // Try to fetch specific profile from C# backend to hydrate customer/pt attributes
        try {
          let phone = '';
          let avatar = '';
          try {
            const userRes = await api.get('/users/me');
            if (userRes.data?.success && userRes.data.data) {
              phone = userRes.data.data.phone || '';
              avatar = userRes.data.data.avatarUrl || '';
            }
          } catch (userErr) {}

          setCurrentUser(prev => prev ? {
            ...prev,
            phoneNumber: phone,
            avatar: avatar
          } : null);

          if (mappedRole === 'customer') {
            const profileRes = await api.get('/customers/me/profile');
            if (profileRes.data && profileRes.data.success && profileRes.data.data) {
              const p = profileRes.data.data;
              setCurrentUser(prev => prev ? {
                ...prev,
                gender: p.gender,
                height: p.heightCm,
                weight: p.weightKg,
                fitnessGoal: p.fitnessGoal,
                healthNotes: p.healthNote
              } : null);
            }
          } else if (mappedRole === 'pt') {
            const profileRes = await api.get('/pts/me/profile');
            if (profileRes.data && profileRes.data.success && profileRes.data.data) {
              const p = profileRes.data.data;
              setCurrentUser(prev => prev ? {
                ...prev,
                bio: p.bio,
                experienceYears: p.experienceYears,
                specialization: p.specialization ? p.specialization.split(',') : [],
                verificationStatus: p.verificationStatus === 3 ? 'verified' : p.verificationStatus === 4 ? 'rejected' : 'pending'
              } : null);
            }
          }
        } catch (profileError) {
          console.warn('Could not fetch detailed profile details:', profileError);
        }

        return { success: true, role: mappedRole };
      }
      return { success: false };
    } catch (e) {
      console.error('Error logging in:', e);
      return { success: false };
    }
  };

  const googleLogin = async (idToken: string, role?: UserRole) => {
    try {
      // role=1 is Customer
      const payload: any = { idToken };
      if (role) {
        payload.role = role === 'pt' ? 2 : (role === 'customer' ? 1 : 3);
      }
      
      const res = await api.post('/auth/google-login', payload);
      if (res.data?.success) {
        const { accessToken, refreshToken, role: returnedRoleNum } = res.data.data;
        const mappedRole = returnedRoleNum === 1 ? 'customer' : (returnedRoleNum === 2 ? 'pt' : 'admin');
        
        localStorage.setItem('lockedin_access_token', accessToken);
        localStorage.setItem('lockedin_refresh_token', refreshToken);
        localStorage.setItem('lockedin_current_role', mappedRole);
        
        // Refresh page or trigger re-fetch logic here
        window.location.reload(); 
        return { success: true, role: mappedRole };
      }
      return { success: false, message: res.data?.message || 'Google login failed' };
    } catch (error: any) {
      console.error('Google login failed:', error);
      return { success: false, message: error.response?.data?.message || 'Lỗi kết nối' };
    }
  };

  const register = async (fullName: string, email: string, role: UserRole, phone: string, password?: string): Promise<User> => {
    try {
      const pass = password || 'Password123';
      let response;
      if (role === 'pt') {
        response = await api.post('/auth/register/pt', {
          email,
          password: pass,
          fullName,
          phone,
          bio: 'Huấn luyện viên thể hình chuyên nghiệp',
          experienceYears: 1,
          specialization: []
        });
      } else {
        response = await api.post('/auth/register/customer', {
          email,
          password: pass,
          fullName,
          phone
        });
      }

      const envelope = response.data;
      const authData = envelope && envelope.success ? envelope.data : null;
      
      // Auto login after successful registration
      await login(email, pass);

      return {
        id: (authData && authData.userId) || (envelope && envelope.userId) || '',
        email,
        fullName,
        role,
        isEmailVerified: true
      };
    } catch (e) {
      console.error('Error registering:', e);
      throw e;
    }
  };

  const logout = () => {
    try {
      api.post('/auth/logout').catch(() => {});
    } catch {}
    setCurrentUser(null);
    setCurrentRole(null);
    localStorage.removeItem('lockedin_session');
  };

  const updateProfile = async (updatedData: Partial<User>): Promise<void> => {
    if (!currentUser) return;

    try {
      // 1. Update general user profile
      const userBody: any = {};
      if (updatedData.fullName !== undefined) userBody.fullName = updatedData.fullName;
      if (updatedData.phoneNumber !== undefined) userBody.phone = updatedData.phoneNumber;

      if (updatedData.fullName !== undefined || updatedData.phoneNumber !== undefined) {
        await api.put('/users/me', userBody);
      }

      // 2. Update role-specific profile
      if (currentRole === 'customer') {
        const body: any = {};
        if (updatedData.gender !== undefined) body.gender = updatedData.gender;
        if (updatedData.height !== undefined) body.heightCm = updatedData.height;
        if (updatedData.weight !== undefined) body.weightKg = updatedData.weight;
        if (updatedData.fitnessGoal !== undefined) body.fitnessGoal = updatedData.fitnessGoal;
        if (updatedData.healthNotes !== undefined) body.healthNote = updatedData.healthNotes;
        
        if (updatedData.height !== undefined || updatedData.weight !== undefined || updatedData.fitnessGoal !== undefined || updatedData.gender !== undefined) {
          await api.put('/customers/me/profile', body);
        }
      } else if (currentRole === 'pt') {
        const body: any = {};
        if (updatedData.bio !== undefined) body.bio = updatedData.bio;
        if (updatedData.experienceYears !== undefined) body.experienceYears = updatedData.experienceYears;
        if (updatedData.bio !== undefined || updatedData.experienceYears !== undefined) {
          await api.put('/pts/me/profile', body);
        }
      }
    } catch (e) {
      console.error('Error updating profile with backend:', e);
    }

    // Sync state locally
    setCurrentUser(prev => prev ? { ...prev, ...updatedData } : null);
  };

  // Switch role for developer dashboard / HUD
  const devSwitchRole = (role: UserRole) => {
    setCurrentRole(role);
    if (currentUser) {
      if (role === 'pt') {
        setCurrentUser({
          ...currentUser,
          role,
          email: 'pttest@gmail.com',
          fullName: 'PT Test Account',
          phoneNumber: '0909 888 777',
          avatar: 'https://images.unsplash.com/photo-1548690312-e3b507d8c110?q=80&w=250&auto=format&fit=crop',
          bio: 'Chuyên gia thể hình với hơn 8 năm kinh nghiệm trong lĩnh vực nâng tạ, tăng cơ và phục hồi chấn thương.',
          experienceYears: 8,
          verificationStatus: 'verified'
        });
      } else if (role === 'admin') {
        setCurrentUser({
          ...currentUser,
          role,
          email: 'admin@lockedin.vn',
          fullName: 'LockedIn Admin',
          phoneNumber: '0912 345 678',
          avatar: 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?q=80&w=250&auto=format&fit=crop'
        });
      } else {
        setCurrentUser({
          ...currentUser,
          role,
          email: 'usertest@gmail.com',
          fullName: 'Customer Test Account',
          phoneNumber: '0901 234 567',
          avatar: 'https://images.unsplash.com/photo-1534438327276-14e5300c3a48?q=80&w=250&auto=format&fit=crop',
          gender: 'male',
          height: 178,
          weight: 78.5,
          fitnessGoal: 'Tăng Cơ Giảm Mỡ',
          healthNotes: 'Không chấn thương'
        });
      }
    }
  };

  const requestVerification = async (personalId: File, certificates: File[], notes?: string) => {
    if (!currentUser || currentUser.role !== 'pt') return;
    console.log('Submitting verification documents. Notes:', notes);

    try {
      // Helper to upload a single file
      const uploadFile = async (file: File) => {
        const formData = new FormData();
        formData.append('file', file);
        const res = await api.post('/uploads/image?folder=documents', formData, {
          headers: { 'Content-Type': 'multipart/form-data' }
        });
        if (res.data?.success) {
          return res.data.data.secureUrl || res.data.data.url;
        }
        throw new Error('File upload failed');
      };

      const personalIdUrl = await uploadFile(personalId);

      await api.post('/pts/me/documents', {
        documentType: 1, // CCCD
        fileUrl: personalIdUrl
      });

      for (const certFile of certificates) {
        const certUrl = await uploadFile(certFile);
        await api.post('/pts/me/documents', {
          documentType: 2, // Certificate
          fileUrl: certUrl
        });
      }

      await updateProfile({
        verificationStatus: 'pending'
      });
    } catch (e) {
      console.error('Error uploading documents:', e);
      throw e;
    }
  };

  return (
    <AuthContext.Provider
      value={{
        currentUser,
        currentRole,
        users,
        login,
        googleLogin,
        register,
        logout,
        updateProfile,
        devSwitchRole,
        requestVerification,
        refreshUsersList
      }}
    >
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => {
  const context = useContext(AuthContext);
  if (!context) throw new Error('useAuth must be used within an AuthProvider');
  return context;
};
