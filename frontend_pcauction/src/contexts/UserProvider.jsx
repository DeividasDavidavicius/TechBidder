import React, { useCallback, useState } from 'react';
import UserContext from './UserContext';

const UserContextProvider = ({ children }) => {
    const [isLoggedIn, setIsLoggedIn] = useState(localStorage.getItem('isLoggedIn') === 'true');
    const [role, setRole] = useState(localStorage.getItem('role') || 'guest');
    const [accessToken, setAccessToken] = useState(localStorage.getItem('accessToken') || 'empty');
    const [refreshToken, setRefreshToken] = useState(localStorage.getItem('refreshToken') || 'empty');

    function decodeJWT(token) {
        try {
            const base64Url = token.split('.')[1];
            const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
            const payload = JSON.parse(atob(base64));

            return payload;
        } catch (error) {
            console.error("Failed to decode JWT:", error);
            return null;
        }
    }

    const setLogin = useCallback((newAccessToken, newRefreshToken) => {
        const payload = decodeJWT(newAccessToken);
        const userRole = payload ? payload['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] : 'none';

        setIsLoggedIn(true);
        setRole(userRole);
        setAccessToken(newAccessToken);
        setRefreshToken(newRefreshToken);

        localStorage.setItem('isLoggedIn', 'true');
        localStorage.setItem('role', userRole);
        localStorage.setItem('accessToken', newAccessToken);
        localStorage.setItem('refreshToken', newRefreshToken);
    }, []);

    const setLogout = useCallback(() => {
        setIsLoggedIn(false);
        setRole('guest');
        setAccessToken('empty');
        setRefreshToken('empty');

        localStorage.removeItem('isLoggedIn');
        localStorage.removeItem('role');
        localStorage.removeItem('accessToken');
        localStorage.removeItem('refreshToken');
    }, []);

    const getUserId = () =>
    {
        const payload = decodeJWT(localStorage.getItem('accessToken'));
        const userId = payload.sub;
        return userId;
    }

    const getUserName = () =>
    {
        const payload = decodeJWT(localStorage.getItem('accessToken'));
        const userName = payload["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"];
        return userName;
    }


    return (
        <UserContext.Provider value={{ isLoggedIn, role, accessToken, refreshToken, decodeJWT, setLogin, setLogout, getUserId, getUserName }}>
            {children}
        </UserContext.Provider>
    );
};

export default UserContextProvider;
