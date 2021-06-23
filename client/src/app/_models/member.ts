import { Photo } from "./photo";


export interface Member {
    id: number;
    username: string;
    photoUrl: string;
    age: number;
    knownAs: string;
    created: Date;
    lastActive: Date;
    genders: string;
    introduction: string;
    lookingFor: string;
    interest: string;
    city: string;
    country: string;
    photos: Photo[];
}

