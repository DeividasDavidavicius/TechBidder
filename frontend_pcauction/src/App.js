import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import React, { useEffect } from 'react';
import './App.css';
import Header from './components/Header';
import AppRoutes from './utils/AppRoutes';

function App() {
  useEffect(() => {
    if (!localStorage.getItem('isLoggedIn')) {
      localStorage.setItem('isLoggedIn', 'false');
    }
    if (!localStorage.getItem('role')) {
      localStorage.setItem('role', 'guest');
    }
    if (!localStorage.getItem('accessToken')) {
      localStorage.setItem('accessToken', 'empty');
    }
    if (!localStorage.getItem('refreshToken')) {
      localStorage.setItem('refreshToken', 'empty');
    }
  }, []);
  return (
    <div className='App'>
      <Router>
        <Header/>
        <Routes>
          {AppRoutes.map((route, index) => {
            return <Route key={index} path={route.path} element={route.element}/>
          })}
        </Routes>
      </Router>
    </div>
  );
}

export default App;
