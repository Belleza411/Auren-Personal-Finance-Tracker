export interface UserResponse {
    email: string;
    firstName: string;
    lastName: string;
    profilePictureUrl: string | null;
    createdAt: Date | string;
    lastLoginAt: Date | string;
}

export interface UserDto {
    email: string;
    firstName: string;
    lastName: string;
    profileImageUploadRequest?: ProfileImageUploadRequest;
    currency: string;
}

export interface ProfileImageUploadRequest {
    file?: File | null;
    name?: string | null;
    description?: string | null;
}
