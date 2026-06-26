// src/services/mockAuth.ts

export type UserRole = 'customer' | 'pt' | 'admin';

export interface User {
  id: string;
  email: string;
  role: UserRole;
  fullName: string;
  phoneNumber?: string;
  avatar?: string;
  isEmailVerified: boolean;
  
  // Customer Profile Specific
  height?: number; // cm
  weight?: number; // kg
  gender?: 'male' | 'female' | 'other';
  fitnessGoal?: string;
  healthNotes?: string;

  // PT Profile Specific
  bio?: string;
  specialization?: string[];
  experienceYears?: number;
  contactInfo?: string;
  isVerified?: boolean; // PT verification status
  verificationStatus?: 'none' | 'pending' | 'verified' | 'rejected' | 'changes_requested';
  verificationDocs?: {
    personalId: string; // CCCD
    certificates: string[];
    notes?: string;
  };
}

const DEFAULT_USERS: User[] = [
  {
    id: 'cust-1',
    email: 'usertest@gmail.com',
    role: 'customer',
    fullName: 'Customer Test Account',
    phoneNumber: '0987654321',
    avatar: 'https://images.unsplash.com/photo-1544005313-94ddf0286df2?auto=format&fit=crop&q=80&w=200',
    isEmailVerified: true,
    height: 165,
    weight: 58,
    gender: 'female',
    fitnessGoal: 'Lose weight & tone muscles',
    healthNotes: 'Slight lower back sensitivity'
  },
  {
    id: 'pt-1',
    email: 'pttest@gmail.com',
    role: 'pt',
    fullName: 'PT Test Account',
    phoneNumber: '0912345678',
    avatar: 'https://images.unsplash.com/photo-1568602471122-7832951cc4c5?auto=format&fit=crop&q=80&w=200',
    isEmailVerified: true,
    bio: 'Certified Strength Coach & Nutrition Specialist. 8+ years helping clients build sustainable physical habits.',
    specialization: ['Weight Loss', 'Hypertrophy', 'Strength Conditioning'],
    experienceYears: 8,
    contactInfo: 'pttest@gmail.com | Zalo: 0912345678',
    isVerified: true,
    verificationStatus: 'verified'
  },
  {
    id: 'pt-2',
    email: 'sarah.pt@lockedin.fit',
    role: 'pt',
    fullName: 'Sarah Jenkins',
    phoneNumber: '0977665544',
    avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?auto=format&fit=crop&q=80&w=200',
    isEmailVerified: true,
    bio: 'Yoga & Pilates instructor focusing on core power, posture adjustment, and high-performance flexibility.',
    specialization: ['Pilates', 'Flexibility', 'Posture Correction'],
    experienceYears: 4,
    contactInfo: 'sarah.j@pt.lockedin.fit',
    isVerified: false,
    verificationStatus: 'pending',
    verificationDocs: {
      personalId: '123456789012',
      certificates: ['International Pilates Cert - Level 2', 'Yoga Alliance RYT-200'],
      notes: 'Please review my credentials'
    }
  },
  {
    id: 'admin-1',
    email: 'admin@lockedin.vn',
    role: 'admin',
    fullName: 'LockedIn Admin',
    isEmailVerified: true
  }
];

// Initialize users table in localStorage if not exists
const USERS_KEY = 'lockedin_users';

export function getMockUsers(): User[] {
  const users = localStorage.getItem(USERS_KEY);
  if (!users) {
    localStorage.setItem(USERS_KEY, JSON.stringify(DEFAULT_USERS));
    return DEFAULT_USERS;
  }
  return JSON.parse(users);
}

export function saveMockUsers(users: User[]): void {
  localStorage.setItem(USERS_KEY, JSON.stringify(users));
}
