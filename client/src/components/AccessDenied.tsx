import React from 'react';

interface NetworkMessageProps {
  title?: string;
  message?: string;
}

const NetworkMessage: React.FC<NetworkMessageProps> = ({
  title = 'Tilgang begrenset',
  message = 'Koble til Ulrikens nettverk for å se denne nettsiden'
}) => {
  return (
    <div style={{
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      height: '100vh',
      textAlign: 'center',
      padding: '20px',
      backgroundColor: '#f8f9fa',
      color: '#343a40'
    }}>
      <div>
        <h1 style={{ fontSize: '24px', marginBottom: '20px' }}>{title}</h1>
        <p style={{ fontSize: '18px', marginBottom: '30px' }}>{message}</p>
        <div style={{
          width: '80px',
          height: '80px',
          margin: '0 auto 30px',
          border: '5px solid #dc3545',
          borderTopColor: 'transparent',
          borderRadius: '50%',
          animation: 'spin 1s linear infinite'
        }} />
      </div>
      <style jsx>{`
        @keyframes spin {
          0% { transform: rotate(0deg); }
          100% { transform: rotate(360deg); }
        }
      `}</style>
    </div>
  );
};

export const AccessDenied: React.FC = () => (
  <NetworkMessage 
    title="Tilgang begrenset"
    message="Koble til Ulrikens nettverk for å se denne nettsiden"
  />
);

export const NotFound: React.FC = () => (
  <NetworkMessage 
    title="404 - Side ikke funnet"
    message="Koble til Ulrikens nettverk for å se denne nettsiden"
  />
);

export default AccessDenied;
