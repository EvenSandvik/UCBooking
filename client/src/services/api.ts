import { API_BASE_URL, API_ENDPOINTS, ROOM_EMAILS, DEFAULT_TIME_SETTINGS } from '../config';

export interface BookingRequest {
  roomId: string;
  userName: string;
  date: string;
  startTime: string;
  endTime: string;
  subject: string;
  content: string;
}

export interface BookingResponse {
  id: string;
  success: boolean;
  message?: string;
}

export const bookRoom = async (bookingData: Omit<BookingRequest, 'roomId'> & { roomId: string }): Promise<BookingResponse> => {
  try {
    const roomEmail = ROOM_EMAILS[bookingData.roomId];
    if (!roomEmail) {
      throw new Error('Invalid room selected');
    }

    const response = await fetch(`${API_BASE_URL}${API_ENDPOINTS.BOOK_ROOM}`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({
        roomEmail,
        subject: bookingData.subject || 'Meeting',
        content: bookingData.content || 'Scheduled meeting',
        start: `${bookingData.date}T${bookingData.startTime}`,
        end: `${bookingData.date}T${bookingData.endTime}`,
        timeZone: DEFAULT_TIME_SETTINGS.timeZone,
      }),
    });

    const data = await response.json();

    if (!response.ok) {
      throw new Error(data.message || 'Failed to book room');
    }

    return {
      id: data.id,
      success: true,
      message: 'Room booked successfully!',
    };
  } catch (error) {
    console.error('Error booking room:', error);
    return {
      id: '',
      success: false,
      message: error instanceof Error ? error.message : 'Failed to book room',
    };
  }
};

// Add more API functions as needed
