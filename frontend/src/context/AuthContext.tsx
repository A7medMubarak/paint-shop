import { useState, useCallback, useEffect, type ReactNode } from 'react';
import { AuthContext } from './AuthContextValue';
import { authService } from '../services/auth';
import type { LoginRequest } from '../types';

function isTokenExpired(token: string): boolean {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp * 1000 < Date.now();
  } catch {
    return true;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(localStorage.getItem('token'));
  const [role, setRole] = useState<string | null>(localStorage.getItem('role'));
  const [username, setUsername] = useState<string | null>(localStorage.getItem('username'));

  useEffect(() => {
    if (token && isTokenExpired(token)) {
      authService.logout();
      setToken(null);
      setRole(null);
      setUsername(null);
    }
  }, [token]);

  const login = useCallback(async (data: LoginRequest) => {
    const response = await authService.login(data);
    authService.storeSession(response);
    setToken(response.token);
    setRole(response.role);
    setUsername(response.username);
    return response;
  }, []);

  const logout = useCallback(() => {
    authService.logout();
    setToken(null);
    setRole(null);
    setUsername(null);
  }, []);

  return (
    <AuthContext.Provider value={{
      token, role, username,
      isOwner: role === 'Owner',
      isAuthenticated: !!token,
      login, logout
    }}>
      {children}
    </AuthContext.Provider>
  );
}
