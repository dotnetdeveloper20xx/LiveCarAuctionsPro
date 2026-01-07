export interface Auction {
  id: string;
  title: string;
  type: AuctionType;
  status: AuctionStatus;
  vehicleId: string;
  vehicle?: Vehicle;
  sellerId: string;
  sellerName?: string;
  startingPrice: Money;
  reservePrice?: Money;
  buyNowPrice?: Money;
  currentHighBid: Money;
  winningBidId?: string;
  winnerId?: string;
  startTime: Date;
  endTime: Date;
  actualEndTime?: Date;
  isDealerOnly: boolean;
  bidCount: number;
  description?: string;
  watcherCount?: number;
  isWatching?: boolean;
  antiSnipingEnabled?: boolean;
  antiSnipingExtensionMinutes?: number;
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
  isSalvage?: boolean;
  imageUrl?: string;
  images?: VehicleImage[];
  conditionReport?: ConditionReport;
}

export interface VehicleImage {
  id: string;
  url: string;
  type: string;
  isPrimary: boolean;
}

export interface ConditionReport {
  overallGrade: string;
  exteriorGrade: string;
  interiorGrade: string;
  mechanicalGrade: string;
  inspectedAt: Date;
  notes?: string;
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
  maxProxyAmount?: Money;
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
  dealerLicenseNumber?: string;
  companyName?: string;
  creditLimit?: Money;
  availableCredit?: Money;
  kycVerified?: boolean;
  status?: string;
  phone?: string;
}

export interface Notification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  auctionId?: string;
  isRead: boolean;
  createdAt: Date;
}

export interface WatchlistItem {
  id: string;
  auctionId: string;
  auction?: Auction;
  addedAt: Date;
}

export interface UserBidSummary {
  auctionId: string;
  auctionTitle: string;
  vehicleInfo: string;
  imageUrl?: string;
  myHighestBid: Money;
  currentHighBid: Money;
  status: 'winning' | 'outbid' | 'won' | 'lost';
  endTime: Date;
  auctionStatus: AuctionStatus;
}

export interface SellerStats {
  totalListings: number;
  activeAuctions: number;
  completedAuctions: number;
  totalSales: Money;
  averagePrice: Money;
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
  Completed = 'Completed',
  Cancelled = 'Cancelled',
  EndedNoSale = 'EndedNoSale'
}

export enum BidStatus {
  Active = 'Active',
  Winning = 'Winning',
  Outbid = 'Outbid',
  Won = 'Won',
  Lost = 'Lost',
  Withdrawn = 'Withdrawn'
}

export enum UserRole {
  Buyer = 'Buyer',
  Seller = 'Seller',
  Dealer = 'Dealer',
  Admin = 'Admin',
  Inspector = 'Inspector'
}

export enum NotificationType {
  Outbid = 'Outbid',
  AuctionWon = 'AuctionWon',
  AuctionEndingSoon = 'AuctionEndingSoon',
  AuctionStarted = 'AuctionStarted',
  BidPlaced = 'BidPlaced',
  PaymentRequired = 'PaymentRequired',
  WatchlistAlert = 'WatchlistAlert'
}

export enum TitleStatus {
  Clean = 'Clean',
  Rebuilt = 'Rebuilt',
  Salvage = 'Salvage',
  Lemon = 'Lemon',
  Flood = 'Flood'
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

export interface SearchFilters {
  make?: string;
  model?: string;
  yearFrom?: number;
  yearTo?: number;
  priceFrom?: number;
  priceTo?: number;
  mileageFrom?: number;
  mileageTo?: number;
  status?: AuctionStatus;
  type?: AuctionType;
  dealerOnly?: boolean;
  searchTerm?: string;
}
