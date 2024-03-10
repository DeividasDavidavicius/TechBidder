import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import React, { useEffect } from 'react';
import './App.css';
import Header from './components/Header';
import MainPage from './components/MainPage';
import Login from './components/auth/Login';
import Register from './components/auth/Register';

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
          <Route path='/' element={<MainPage/>}></Route>
          <Route path='/login' element={<Login />}></Route>
          <Route path='/register' element={<Register />}></Route>
        </Routes>
      </Router>
    </div>
  );
}

export default App;
