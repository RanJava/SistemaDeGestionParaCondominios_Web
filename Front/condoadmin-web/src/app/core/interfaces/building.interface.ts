export interface Building{
    id: number;
    name: string;
    address: string;
    city: string;
    totalUnits: number;
    isActive: boolean;
}

export interface CreateBuilding {
    name: string;
    address: string;
    city: string;
}