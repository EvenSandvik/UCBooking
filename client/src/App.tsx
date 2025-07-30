import { useState, useEffect } from 'react';
import './App.css';
import { Room, RoomLegend } from './components/Room';
import { bookRoom } from './services/api';
import { DEFAULT_TIME_SETTINGS } from './config';

interface ResourceBooking {
  id: string;
  resourceType: 'desk' | 'room';
  resourceId: number | string;
  userName: string;
  date: string;
  startTime: string;
  endTime: string;
}

interface Room {
  id: string;
  name: string;
  capacity: number;
  position: {
    top: string;
    left: string;
    width: string;
    height: string;
  };
}

const MEETING_ROOMS: Room[] = [
  {
    id: 'ulriken',
    name: 'Ulriken',
    capacity: 8,
    position: { top: '20px', left: '721px', width: '537px', height: '380px' }
  },
  {
    id: 'rundemannen',
    name: 'Rundemannen',
    capacity: 20,
    position: { top: '20px', left: '20px', width: '678px', height: '380px' }
  },
  {
    id: 'loddefjord',
    name: 'Loddefjord',
    capacity: 2,
    position: { top: '1045px', left: '1093px', width: '166px', height: '253px' }
  },
  {
    id: 'fana',
    name: 'Fana',
    capacity: 2,
    position: { top: '1494px', left: '20px', width: '234px', height: '304px' }
  },
  {
    id: 'asane',
    name: 'Åsane',
    capacity: 2,
    position: { top: '1494px', left: '277px', width: '234px', height: '304px' }
  }
];

function App() {
  const [selectedResource, setSelectedResource] = useState<{type: 'desk' | 'room', id: number | string} | null>(null);
  const [userName, setUserName] = useState('');
  const [bookings, setBookings] = useState<ResourceBooking[]>([]);
  const [loading, setLoading] = useState(true);
  const [date, setDate] = useState(new Date().toISOString().split('T')[0]);
  const [startTime, setStartTime] = useState('09:00');
  const [duration, setDuration] = useState<30 | 60 | 120>(60); // 30min, 1h, or 2h
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState<'success' | 'error'>('success');
  const [showBookingModal, setShowBookingModal] = useState(false);
  
  // Generate time slots from 9:00 to 16:30 in 30-minute intervals
  const timeSlots = Array.from({ length: 16 }, (_, i) => {
    const hours = 9 + Math.floor(i * 0.5);
    const minutes = i % 2 === 0 ? '00' : '30';
    const time = `${hours.toString().padStart(2, '0')}:${minutes}`;
    return {
      value: time,
      label: time
    };
  });

  useEffect(() => {
    fetchBookings();
  }, [date]);

  const fetchBookings = async () => {
    try {
      setLoading(true);
      const response = await fetch(`/api/bookings?date=${date}`);
      const data = await response.json();
      setBookings(data);
    } catch (error) {
      console.error('Error fetching bookings:', error);
      showMessage('Failed to load bookings', 'error');
    } finally {
      setLoading(false);
    }
  };

  const showMessage = (msg: string, type: 'success' | 'error') => {
    setMessage(msg);
    setMessageType(type);
    setTimeout(() => setMessage(''), 5000);
  };

  const handleResourceClick = (type: 'desk' | 'room', id: number | string) => {
    const existingBooking = bookings.find(b => 
      b.resourceType === type && 
      b.resourceId === id && 
      b.date === date
    );
    
    if (existingBooking) {
      showMessage(
        `${type === 'desk' ? 'Desk' : 'Room'} ${id} is booked by ${existingBooking.userName}`, 
        'error'
      );
      return;
    }
    
    setSelectedResource({ type, id });
    setShowBookingModal(true);
  };

  // Calculate end time based on start time and duration
  const calculateEndTime = (start: string, duration: number | string) => {
    // If it's a desk booking with predefined slots
    if (typeof duration === 'string' && duration.includes('-')) {
      return duration.split('-')[1];
    }
    
    // For room bookings with duration in minutes
    const [hours, minutes] = start.split(':').map(Number);
    const date = new Date();
    date.setHours(hours, minutes, 0, 0);
    date.setMinutes(date.getMinutes() + (duration as number));
    
    // Format back to HH:MM
    return `${String(date.getHours()).padStart(2, '0')}:${String(date.getMinutes()).padStart(2, '0')}`;
  };

  const handleBooking = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!selectedResource || !userName.trim()) return;

    const endTime = calculateEndTime(startTime, duration);
    
    try {
      const result = await bookRoom({
        roomId: selectedResource.id.toString(),
        userName: userName.trim(),
        date,
        startTime,
        endTime,
        subject: `Meeting with ${userName}`,
        content: 'Scheduled meeting',
      });

      if (result.success) {
        const newBooking = {
          id: result.id,
          resourceType: selectedResource.type,
          resourceId: selectedResource.id,
          userName: userName.trim(),
          date,
          startTime,
          endTime,
        };
        
        setBookings([...bookings, newBooking]);
        setSelectedResource(null);
        setShowBookingModal(false);
        setUserName('');
        showMessage(result.message || 'Room booked successfully!', 'success');
      } else {
        throw new Error(result.message || 'Booking failed');
      }
    } catch (error) {
      console.error('Error creating booking:', error);
      showMessage(error instanceof Error ? error.message : 'Failed to create booking', 'error');
    }
  };

  const isDeskBooked = (deskNumber: number) => {
    return bookings.some(booking => 
      booking.resourceType === 'desk' &&
      booking.resourceId === deskNumber && 
      booking.date === date
    );
  };

  if (loading) {
    return <div className="loading">Loading...</div>;
  }

  // Helper function to render straight walls
  const renderWall = (position: any, orientation: 'horizontal' | 'vertical') => {
    return (
      <div 
        className={`wall ${orientation}`}
        style={{
          position: 'absolute',
          backgroundColor: '#555',
          ...position
        }}
      />
    );
  };

  // Helper function to render diagonal walls (in degrees)
  const renderDiagonalWall = ({ top, left, width, angle = 45 }: { top: string; left: string; width: string; angle?: number }) => {
    return (
      <div 
        className="wall diagonal"
        style={{
          position: 'absolute',
          backgroundColor: '#555',
          top,
          left,
          width,
          height: '20px',
          transform: `rotate(${angle}deg)`,
          transformOrigin: 'left center'
        }}
      />
    );
  };

  // Helper function to render room labels
  const renderRoomLabel = (text: string, position: { top: string | number; left: string | number }) => {
    return (
      <div 
        className="room-label"
        style={{
          top: position.top,
          left: position.left,
        }}
      >
        {text}
      </div>
    );
  };

  return (
    <div className="app">
      <header className="header">
        <h1>Kokstadflaten 5</h1>
      </header>

      {message && (
        <div className={`message ${messageType}`}>
          {message}
        </div>
      )}

      <div className="desk-grid">
        {/* Office Walls */}
        {/* Outer walls */}
        {renderWall({ top: 0, left: 0, width: '100%', height: '20px' }, 'horizontal')} {/* Top wall */}
        {renderWall({ top: 0, right: 0, width: '20px', height: '85%' }, 'vertical')}   {/* Right wall */}
        {renderWall({ bottom: 0, left: 0, width: '50%', height: '20px' }, 'horizontal')} {/* Bottom wall */}
        {renderWall({ top: 0, left: 0, width: '20px', height: '100%' }, 'vertical')}    {/* Left wall */}

        {renderWall({ top: 400, left: 0, width: '100%', height: '20px' }, 'horizontal')}
        {renderWall({ top: 0, left: 700, width: '20px', height: '400px' }, 'vertical')}
        {renderWall({ top: 820, left: 0, width: 520, height: '20px' }, 'horizontal')}
        
        <RoomLegend />
        <div className="office-map">
          {/* Render meeting rooms */}
          {MEETING_ROOMS.map(room => {
            const isBooked = bookings.some(
              b => b.resourceType === 'room' && 
                   b.resourceId === room.id && 
                   b.date === date
            );
            return (
              <Room
                key={room.id}
                room={room}
                isBooked={isBooked}
                onSelect={() => handleResourceClick('room', room.id)}
              />
            );
          })}
          
          {/* Walls and room labels */}
          {renderWall({ top: '16%', left: '70%', width: '20px', height: '25%' }, 'vertical')}  
          {renderWall({ top: '41%', left: '70%', width: '30%', height: '20px' }, 'horizontal')}
          {renderWall({ top: '41%', left: '84%', width: '20px', height: '12%' }, 'vertical')}
          {renderWall({ top: '52%', left: '60%', width: '40%', height: '15rem' }, 'horizontal')}
          {renderWall({ top: '54%', left: '60%', width: '20px', height: '49rem' }, 'vertical')}
          {renderWall({ top: '85%', left: '60%', width: '40%', height: '20px' }, 'horizontal')}
          {renderWall({ top: '67%', left: '60%', width: '40%', height: '10px' }, 'horizontal')}
          {renderWall({ top: '60%', left: '73%', width: '10px', height: '11rem' }, 'vertical')}
          {renderWall({ top: '60%', left: '86%', width: '10px', height: '11rem' }, 'vertical')}
          {renderWall({ top: '59%', left: '20%', width: '20px', height: '13%' }, 'vertical')}
          {renderWall({ top: '59%', left: '40%', width: '20px', height: '345px' }, 'vertical')}
          {renderWall({ top: '59%', left: '0%', width: '40%', height: '20px' }, 'horizontal')}
          {renderWall({ top: '72%', left: '0%', width: '40%', height: '20px' }, 'horizontal')}
          {renderWall({ top: '90.655%', left: '49.5%', width: '20px', height: '279px' }, 'vertical')}
          {renderDiagonalWall({ 
            top: '90.5%', 
            left: '640px', 
            width: '16%',
            angle: -45 // Negative angle for left-bottom to right-top diagonal
          })}
          {/* Original straight wall (commented out for reference)
          {renderWall({ top: '72%', left: '0%', width: '41%', height: '20px' }, 'horizontal')}
          */}

          {/* Room labels */}
          {renderRoomLabel('Ulriken', { top: '8%', left: '73%' })}
          {renderRoomLabel('Rundemannen', { top: '8%', left: '18%' })}
          {renderRoomLabel('Stue', { top: '24%', left: '18%' })}
          <div className="room-label vertical" style={{ top: '25%', left: '84%' }}>Ledergruppen</div>
          <div className="room-label vertical" style={{ top: '43%', left: '90%' }}>Loddefjord</div>
          {renderRoomLabel('Fana', { top: '65%', left: '8%' })}
          {renderRoomLabel('Åsane', { top: '65%', left: '27%' })}
          {renderRoomLabel('Kjøkken', { top: '46%', left: '65%' })}
          <div className="wc-icon" style={{ position: 'absolute', top: '75%', left: '77%', color: '#e0e0e0' }}>WC</div>
          <div className="wc-icon" style={{ position: 'absolute', top: '63.5%', left: '91.5%', color: '#e0e0e0' }}><i class="fa-solid fa-person-dress"></i></div>
          <div className="wc-icon" style={{ position: 'absolute', top: '63.5%', left: '78.5%', color: '#e0e0e0'  }}><i class="fa-solid fa-person"></i></div>
          <div className="wc-icon" style={{ position: 'absolute', top: '63.5%', left: '66%', color: '#e0e0e0'  }}><i class="fa-solid fa-person"></i></div>
          
          {/* Desks */}
          {[ 
            { number: 1, top: '95%', left: '10%' },
            { number: 2, top: '95%', left: '25%' },
            { number: 3, top: '90%', left: '10%' },
            { number: 4, top: '90%', left: '25%' },
            { number: 5, top: '82%', left: '10%' },
            { number: 6, top: '82%', left: '25%' },
            { number: 7, top: '77%', left: '10%' },
            { number: 8, top: '77%', left: '25%' },
            { number: 9, top: '55%', left: '10%' },
            { number: 10, top: '55%', left: '25%' },
            { number: 11, top: '50%', left: '10%' },
            { number: 12, top: '50%', left: '25%' },
            { number: 13, top: '43%', left: '10%' },
            { number: 14, top: '43%', left: '25%' },
            { number: 15, top: '38%', left: '10%' },
            { number: 16, top: '38%', left: '25%' },
          ].map(({ number, top, left }) => {
            const isBooked = isDeskBooked(number);
            const booking = bookings.find(b => b.deskNumber === number);
            
            return (
              <div
                key={number}
                className={`desk ${isBooked ? 'booked' : 'available'}`}
                onClick={() => handleResourceClick('desk', number)}
                style={{
                  position: 'absolute',
                  top,
                  left,
                  transform: 'translate(-50%, -50%)',
                }}
              >
                <div className="desk-number">{number}</div>
                {isBooked && (
                  <div className="booking-details">
                    <div className="user-name">
                      {bookings.find(b => 
                        b.resourceType === 'desk' && 
                        b.resourceId === number &&
                        b.date === date
                      )?.userName || 'Booked'}
                    </div>
                  </div>
                )}
              </div>
            );
          })}
        </div>
      </div>

      {showBookingModal && selectedResource && (
        <div className="booking-modal-overlay" onClick={(e) => e.target === e.currentTarget && setShowBookingModal(false)}>
          <div className="modal">
            <h2>Book {selectedResource.type === 'desk' ? 'Desk' : 'Meeting Room'} {selectedResource.id}</h2>
            <form onSubmit={handleBooking}>
              <div className="form-group">
                <label htmlFor="userName">Your Name</label>
                <input
                  type="text"
                  id="userName"
                  value={userName}
                  onChange={(e) => setUserName(e.target.value)}
                  required
                  placeholder="Enter your full name"
                  autoFocus
                />
              </div>
              <div className="form-group">
                <label>Date</label>
                <input
                  type="date"
                  value={date}
                  onChange={(e) => setDate(e.target.value)}
                  min={new Date().toISOString().split('T')[0]}
                  required
                />
              </div>
              {selectedResource.type === 'desk' ? (
                <div className="form-group">
                  <label>Time Slot</label>
                  <div className="time-options">
                    {[
                      { value: '09:00-17:00', label: 'Full Day (9:00 - 17:00)' },
                      { value: '09:00-12:30', label: 'Morning (9:00 - 12:30)' },
                      { value: '13:00-17:00', label: 'Afternoon (13:00 - 17:00)' }
                    ].map((slot) => (
                      <div 
                        key={slot.value}
                        className={`time-option ${startTime === slot.value ? 'selected' : ''}`}
                        onClick={() => setStartTime(slot.value)}
                      >
                        {slot.label}
                      </div>
                    ))}
                  </div>
                </div>
              ) : (
                <div className="form-group">
                  <label>Start Time</label>
                  <select
                    className="time-select"
                    value={startTime}
                    onChange={(e) => setStartTime(e.target.value)}
                    required
                  >
                    {timeSlots.map((slot) => (
                      <option key={slot.value} value={slot.value}>
                        {slot.label}
                      </option>
                    ))}
                  </select>
                </div>
              )}
              {selectedResource.type === 'room' && (
                <div className="form-group">
                  <label>Duration</label>
                  <div className="duration-options">
                    {[30, 60, 120].map((minutes) => (
                      <div 
                        key={minutes}
                        className={`duration-option ${duration === minutes ? 'selected' : ''}`}
                        onClick={() => setDuration(minutes as 30 | 60 | 120)}
                      >
                        {minutes} min
                      </div>
                    ))}
                  </div>
                  <div className="duration-preview">
                    {startTime} - {calculateEndTime(startTime, duration)}
                  </div>
                </div>
              )}
              <div className="form-actions">
                <button 
                  type="button" 
                  className="secondary"
                  onClick={() => setShowBookingModal(false)}
                >
                  Cancel
                </button>
                <button 
                  type="submit"
                  className="primary"
                >
                  Book {selectedResource.type === 'desk' ? 'Desk' : 'Room'}
                </button>
              </div>
            </form>
          </div>
        </div>
      )}
    </div>
  );
}

export default App;
