export interface LoginDto {
  userName: string;
  password: string;
}

export interface RegisterDto {
  userName: string;
  password: string;
}

export interface AuthResponse {
  userId: number;
  userName: string;
}
