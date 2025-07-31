// API configuration
export const API_BASE_URL = 'http://localhost:7202/api'; // Azure Functions local development URL

export const API_ENDPOINTS = {
  BOOK_ROOM: '/BookRoom',
  // Add other endpoints as needed
} as const;

// Room email mapping
export const ROOM_EMAILS: Record<string, string> = {
  // Desks
  'kontorplass 1': 'kontorplass 1',
  'kontorplass 2': 'kontorplass 2',
  'kontorplass 3': 'kontorplass 3',
  'kontorplass 4': 'kontorplass 4',
  'kontorplass 5': 'kontorplass 5',
  'kontorplass 6': 'kontorplass 6',
  'kontorplass 7': 'kontorplass 7',
  'kontorplass 8': 'kontorplass 8',
  'kontorplass 9': 'kontorplass 9',
  'kontorplass 10': 'kontorplass 10',
  'kontorplass 11': 'kontorplass 11',
  'kontorplass 12': 'kontorplass 12',
  'kontorplass 13': 'kontorplass 13',
  'kontorplass 14': 'kontorplass 14',
  'kontorplass 15': 'kontorplass 15',
  'kontorplass 16': 'kontorplass 16',
  
  // Meeting rooms 
  ulriken: 'Møterom - Ulriken',
  rundemannen: 'Møterom - Rundemannen',
  loddefjord: 'Stillerom - Loddefjord',
  fana: 'Stillerom - Fana',
};

// Default time settings
export const DEFAULT_TIME_SETTINGS = {
  startTime: '09:00',
  endTime: '17:00',
  timeZone: 'W. Europe Standard Time',
} as const;
