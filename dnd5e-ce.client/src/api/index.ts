import axios, { AxiosError, AxiosInstance, AxiosRequestConfig, AxiosResponse } from 'axios';

const getCsrfToken = (): string | null => {
  const cookies = document.cookie.split(';');
  for (let cookie of cookies) {
    const [name, value] = cookie.trim().split('=');
    if (name === 'csrf_token') {
      return value;
    }
  }
  return null;
};

const api: AxiosInstance = axios.create({
  baseURL: import.meta.env.VITE_API_URL,
  withCredentials: true, // Important for HttpOnly cookies
});

api.interceptors.request.use(
  (config) => {
    if (config.method && typeof config.method === 'string' && config.method.toLowerCase() === 'post') {
      const csrfToken = getCsrfToken();
      if (csrfToken) {
        config.headers['X-CSRF-Token'] = csrfToken;
      }
    }
    return config;
  },
  (error) => Promise.reject(error)
);

let isRefreshing = false;
let failedQueue: Array<{ resolve: (value: unknown) => void; reject: (reason?: any) => void }> = [];

// Функция для обработки очереди запросов
const processQueue = (error: AxiosError | null, token: string | null = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

// Интерцептор ответа: обработка 401 и обновление токена
api.interceptors.response.use(
  (response: AxiosResponse) => response,
  async (error: unknown) => {
    if (!(error instanceof AxiosError)) {
      console.error('Non-Axios error:', error);
      return Promise.reject(error);
    }

    const originalRequest = error.config as AxiosRequestConfig & { _retry?: boolean };

    // Проверяем, что это 401 и не повторный запрос
    if (error.response?.status !== 401 || originalRequest._retry || originalRequest.url?.endsWith('/auth/refresh-token')) {
      return Promise.reject(error);
    }

    if (isRefreshing) {
      // Добавляем запрос в очередь, если обновление уже выполняется
      return new Promise((resolve, reject) => {
        failedQueue.push({ resolve, reject });
      })
        .then(() => {
          return api(originalRequest);
        })
        .catch((err) => Promise.reject(err));
    }

    originalRequest._retry = true;
    isRefreshing = true;

    try {
      console.log('Attempting to refresh token');
      await api.post('/auth/refresh-token'); // Используем тот же api для CSRF
      console.log('Token refreshed successfully');
      processQueue(null);
      return api(originalRequest); // Повторяем оригинальный запрос
    } catch (refreshError) {
      console.error('Failed to refresh token:', refreshError);
      processQueue(refreshError instanceof AxiosError ? refreshError : new AxiosError('Token refresh failed'));
      // Перенаправляем на страницу логина
      window.location.href = '/login';
      return Promise.reject(refreshError);
    } finally {
      isRefreshing = false;
    }
  }
);

export default api;