export interface Auction {
  id: string;
  title: string;
  type: AuctionType;
  status: AuctionStatus;
  vehicleId: string;
  vehicle?: Vehicle;
  sellerId: string;
  startingPrice: Money;
  reservePrice?: Money;
  buyNowPrice?: Money;
  currentHighBid: Money;
  winningBidId?: string;
  startTime: Date;
  endTime: Date;
  actualEndTime?: Date;
  isDealerOnly: boolean;
  bidCount: number;
}

export interface Vehicle {
  id: string;
  vin: string;
  make: string;
  model: string;
  year: number;
  mileage: number;
  exteriorColor?: string;
  interiorColor?: string;
  engineType?: string;
  transmission?: string;
  fuelType?: string;
  titleStatus: string;
  imageUrl?: string;
}

export interface Bid {
  id: string;
  auctionId: string;
  bidderId: string;
  bidderName?: string;
  amount: Money;
  status: BidStatus;
  placedAt: Date;
  isProxyBid: boolean;
}

export interface Money {
  amount: number;
  currency: string;
}

export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  roles: UserRole[];
  isDealer: boolean;
  creditLimit?: Money;
}

export enum AuctionType {
  Live = 'Live',
  Timed = 'Timed',
  BuyNow = 'BuyNow'
}

export enum AuctionStatus {
  Draft = 'Draft',
  Scheduled = 'Scheduled',
  Active = 'Active',
  Closed = 'Closed',
  Cancelled = 'Cancelled'
}

export enum BidStatus {
  Active = 'Active',
  Winning = 'Winning',
  Outbid = 'Outbid',
  Withdrawn = 'Withdrawn'
}

export enum UserRole {
  Buyer = 'Buyer',
  Seller = 'Seller',
  Dealer = 'Dealer',
  Admin = 'Admin'
}

export interface PaginatedResult<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

export interface ApiResponse<T> {
  data?: T;
  errors?: ApiError[];
  isSuccess: boolean;
}

export interface ApiError {
  code: string;
  description: string;
}
