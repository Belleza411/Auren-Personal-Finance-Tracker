interface User {
    userId: string;
    email: string;
    firstName: string;
    lastName: string;
    profilePictureUrl: string | null;
    createdAt: Date;
    lastLoginAt: Date | null;
}

interface RefreshToken {
    refreshTokenId: string;
    token: string;
    expiryDate: string;
    isExpired: boolean;
    createdAt: Date;
    isRevoked: boolean;
    replacedByToken: string;
    reasonRevoked: string;
    isActive: boolean;
    userId: string;
    user: User;
}

interface AuthResponse {
    success: boolean;
    message: string;
    user: User | null;
    errors: string[] | null
}

interface Register {
    email: string;
    password: string;
    confirmPassword: string;
    firstName: string;
    lastName: string;
    profileImage?: ProfileImageUpload | null;
}

interface ProfileImageUpload {
    file: File | null;
    name: string | null;
    description: string | null;
}

interface Login {
    email: string;
    password: string;
}

export type {
    User,
    RefreshToken,
    AuthResponse,
    Register,
    Login
}