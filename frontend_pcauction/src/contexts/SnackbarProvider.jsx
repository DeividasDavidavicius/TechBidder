import React, { useState } from 'react';
import SnackbarContext from './SnackbarContext';
import Snackbar from '@mui/material/Snackbar';
import Alert from "@mui/material/Alert";

export const SnackbarProvider = ({ children }) => {
  const [snackbarOpen, setSnackbarOpen] = useState(false);
  const [snackbarMessage, setSnackbarMessage] = useState('');
  const [snackbarLevel, setSnackbarLevel] = useState('success');

  const openSnackbar = (message, level = 'success') => {
    setSnackbarMessage(message);
    setSnackbarLevel(level);
    setSnackbarOpen(true);
  };

  return (
    <SnackbarContext.Provider value={openSnackbar}>
      {children}
      <Snackbar
          open={snackbarOpen}
          autoHideDuration={5000}
          onClose={() => setSnackbarOpen(false)}
          anchorOrigin={{ vertical: "top", horizontal: 'left' }}
          style={{ top: 80}}
      >
          <Alert onClose={() => setSnackbarOpen(false)} severity={snackbarLevel} variant="filled" sx={{ fontSize: '1rem', padding: '10px' }}>
              {snackbarMessage}
          </Alert>
      </Snackbar>
    </SnackbarContext.Provider>
  );
};
