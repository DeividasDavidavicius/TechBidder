import React, { useState, useEffect } from 'react';
import { Typography } from '@mui/material';

const CountdownTimer = ({ targetDate }) => {
  const calculateTimeLeft = () => {
    const difference = +new Date(targetDate) - +new Date();
    let timeLeft = {};

    if (difference > 0) {
      timeLeft = {
        d: Math.floor(difference / (1000 * 60 * 60 * 24)),
        h: Math.floor((difference / (1000 * 60 * 60)) % 24),
        min: Math.floor((difference / 1000 / 60) % 60),
        s: Math.floor((difference / 1000) % 60)
      };
    }

    return timeLeft;
  };

  const [timeLeft, setTimeLeft] = useState(calculateTimeLeft());

  useEffect(() => {
    const timer = setTimeout(() => {
      setTimeLeft(calculateTimeLeft());
    }, 1000);

    return () => clearTimeout(timer);
  });

  const timerComponents = [];

  Object.keys(timeLeft).forEach(interval => {
    if (!timeLeft[interval]) {
      return;
    }

    timerComponents.push(
      <Typography key={interval}
        variant="subtitle1"
        sx={{
            marginRight: '5px', fontWeight: 'bold',
            fontSize: '20px',
            fontFamily: 'Arial, sans-serif',
            color: '#333',
            letterSpacing: '1px'
        }}
      >
        {timeLeft[interval]}{interval}.
      </Typography>
    );
  });

  return (
    <div style={{ display: 'flex', alignItems: 'center', marginLeft: '10px' }}>
        {' '}
      {timerComponents.length ? (
        timerComponents
      ) : (
        <Typography variant="h6"></Typography>
      )}
    </div>
  );
};

export default CountdownTimer;
