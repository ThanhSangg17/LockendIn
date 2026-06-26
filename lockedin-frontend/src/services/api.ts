import axios from 'axios';

// Create configured Axios instance
const api = axios.create({
  baseURL: '/api',
  headers: {
    'Content-Type': 'application/json',
  },
});

// Request interceptor to attach JWT Token
api.interceptors.request.use(
  (config) => {
    const savedSession = localStorage.getItem('lockedin_session');
    if (savedSession) {
      try {
        const { accessToken } = JSON.parse(savedSession);
        if (accessToken) {
          config.headers.Authorization = `Bearer ${accessToken}`;
        }
      } catch (e) {
        console.error('Error parsing auth session token:', e);
      }
    }
    return config;
  },
  (error) => {
    return Promise.reject(error);
  }
);

let isRefreshing = false;
let failedQueue: Array<{ resolve: (value?: unknown) => void; reject: (reason?: any) => void }> = [];

const processQueue = (error: any, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Response interceptor to handle unauthorized access
api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (error.response?.status === 401 && !originalRequest._retry) {
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then((token) => {
            originalRequest.headers.Authorization = `Bearer ${token}`;
            return api(originalRequest);
          })
          .catch((err) => {
            return Promise.reject(err);
          });
      }

      originalRequest._retry = true;
      isRefreshing = true;

      const savedSession = localStorage.getItem('lockedin_session');
      if (savedSession) {
        try {
          const sessionObj = JSON.parse(savedSession);
          const refreshToken = sessionObj.refreshToken;
          
          if (refreshToken) {
            // Must use raw axios to avoid infinite loop of interceptors
            const res = await axios.post('/api/auth/refresh', { refreshToken });
            if (res.data?.success && res.data.data) {
              const newAuth = res.data.data;
              localStorage.setItem('lockedin_session', JSON.stringify({
                ...sessionObj,
                accessToken: newAuth.accessToken,
                refreshToken: newAuth.refreshToken
              }));
              api.defaults.headers.common['Authorization'] = `Bearer ${newAuth.accessToken}`;
              originalRequest.headers.Authorization = `Bearer ${newAuth.accessToken}`;
              processQueue(null, newAuth.accessToken);
              return api(originalRequest);
            }
          }
        } catch (refreshError) {
          processQueue(refreshError, null);
          console.warn('Refresh token failed. Clearing session.');
          localStorage.removeItem('lockedin_session');
          window.dispatchEvent(new Event('auth_logout'));
          return Promise.reject(refreshError);
        } finally {
          isRefreshing = false;
        }
      }

      processQueue(error, null);
      isRefreshing = false;
      console.warn('Unauthorized request detected. Clearing session.');
      localStorage.removeItem('lockedin_session');
      window.dispatchEvent(new Event('auth_logout'));
    }
    
    return Promise.reject(error);
  }
);

// MealPlans API
export const mealPlans = {
  getPlans: () => api.get('/meal-plans'),
  getPlan: (id: string) => api.get(`/meal-plans/${id}`),
  createPlan: (data: any) => api.post('/meal-plans', data),
};

// Conversations API
export const conversations = {
  getConversations: () => api.get('/conversations'),
  getConversation: (id: string) => api.get(`/conversations/${id}`),
  sendMessage: (id: string, message: string) => api.post(`/conversations/${id}/messages`, { message }),
};

// We export the raw axios instance as default so components can use api.get() directly
export default api;
