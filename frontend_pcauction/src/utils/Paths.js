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
    SERIES: '/admin/series',
    CREATESERIES: '/admin/series/create',
    EDITSERIES: '/admin/categories/:categoryId/series/:seriesId',
    PARTREQUESTS: '/admin/parts/requests',
    PARTREQUESTCREATE: '/admin/categories/requests/:categoryId/parts/:partId',
    ANY: '*'
}

export default PATHS;
