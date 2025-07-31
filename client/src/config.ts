// API configuration
export const API_BASE_URL = 'http://localhost:7202/api'; // Azure Functions local development URL

export const API_ENDPOINTS = {
  BOOK_ROOM: '/BookRoom',
  // Add other endpoints as needed
} as const;

// Room email mapping
export const ROOM_EMAILS: Record<string, string> = {
  ulriken: 'ulriken@uc.no', // Replace with actual room emails
  rundemannen: 'rundemannen@uc.no',
  loddefjord: 'loddefjord@uc.no',
  fana: 'fana@uc.no',
};

// Default time settings
export const DEFAULT_TIME_SETTINGS = {
  startTime: '09:00',
  endTime: '17:00',
  timeZone: 'W. Europe Standard Time',
} as const;
