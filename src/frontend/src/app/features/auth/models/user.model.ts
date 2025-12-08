interface User {
    userId: string;
    firstName: string;
    lastName: string;
    profilePictureUrl: string | null;
    createdAt: Date;
    lastLoginAt: Date | null;
    currency: string | null;
    refreshTokens: RefreshToken[]
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

export type {
    User,
    RefreshToken
}