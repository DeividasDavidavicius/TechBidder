const PATHS = {
    MAIN: '/',
    LOGIN: '/login',
    REGISTER: '/register',
    AUCTIONS: '/auctions',
    AUCTIONINFO: '/auctions/:auctionId',
    CREATEAUCTION: '/auctions/create',
    EDITAUCTION: '/auctions/edit/:auctionId',
    PARTS: '/admin/parts',
    CREATEPART: '/admin/parts/create',
    EDITPART: '/admin/categories/:categoryId/parts/:partId',
    ANY: '*'
}

export default PATHS;
