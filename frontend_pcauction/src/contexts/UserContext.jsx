import React, { useContext } from 'react';

const UserContext = React.createContext({
    isLoggedIn: false,
    role: 'guest',
    setLogin: () => {},
    setLogout: () => {},
});

export const useUser = () => {
    return useContext(UserContext);
}

export default UserContext;
