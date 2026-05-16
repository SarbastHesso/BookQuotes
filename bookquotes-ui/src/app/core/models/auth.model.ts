export interface LoginDto {
  userName: string;
  password: string;
}

export interface RegisterDto {
  userName: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  userId: number;
  userName: string;
}
