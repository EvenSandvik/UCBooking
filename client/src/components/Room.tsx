import React from 'react';

interface RoomProps {
  room: {
    id: string;
    name: string;
    capacity: number;
    position: {
      top: string;
      left: string;
      width: string;
      height: string;
    };
  };
  isBooked: boolean;
  onSelect: (roomId: string) => void;
}

export const Room: React.FC<RoomProps> = ({ room, isBooked, onSelect }) => {
  // Determine room type based on capacity
  const getRoomType = () => {
    if (room.capacity >= 15) return 'Auditorium';
    if (room.capacity <= 2) return 'Stillerom';
    return 'Møterom';
  };

  return (
    <div 
      className={`room ${isBooked ? 'booked' : 'available'}`}
      style={{
        position: 'absolute',
        top: room.position.top,
        left: room.position.left,
        width: room.position.width,
        height: room.position.height,
      }}
      onClick={() => !isBooked && onSelect(room.id)}
    >
      <div className="room-type">{getRoomType()}</div>
      {isBooked && <div className="room-status">Booked</div>}
    </div>
  );
};

export const RoomLegend: React.FC = () => {
  return (
    <div className="room-legend">
      <div className="legend-item">
        <div className="legend-color legend-desk"></div>
      </div>
      <div className="legend-item">
        <div className="legend-color legend-room"></div>
      </div>
    </div>
  );
};
