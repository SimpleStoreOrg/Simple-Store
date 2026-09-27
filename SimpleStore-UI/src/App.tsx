import CustomerCart from './pages/Customer/Cart'
import Orders from './pages/ShopperAssistant/Orders'
import OrderHistory from './pages/Customer/OrderHistory'
import CustomerProfile from './pages/Customer/Profile'

import {
    BrowserRouter,
    Routes,
    Route,
} from 'react-router-dom'

import Login from './pages/Login'
import Register from './pages/Register'

import Customers from './pages/Admin/Customers'
import ShopperAssistants from './pages/Admin/ShopperAssistants'
import Products from './pages/Admin/Products'
import AdminOrders from './pages/Admin/Orders'
import Markets from './pages/Admin/Markets'
import Categories from './pages/Admin/Categories'
import Admins from './pages/Admin/Admins'
import Dashboard from './pages/Admin/Dashboard'
import MyMarket from './pages/Admin/MyMarket'
import AdminProfile from './pages/Admin/Profile'

import ShopperAssistantDashboard from './pages/ShopperAssistant/Dashboard'
import ShopperAssistantProfile from './pages/ShopperAssistant/Profile'
import CustomerDashboard from './pages/Customer/Dashboard'
import CustomerProducts from './pages/Customer/Products'

import ProtectedRoute from './components/ProtectedRoute'
import RoleRedirect from './components/RoleRedirect'
import AdminLayout from './components/AdminLayout'
import ShopperAssistantLayout from './components/ShopperAssistantLayout'
import CustomerLayout from './components/CustomerLayout'

function App() {
    return (
        <BrowserRouter>
            <Routes>
                <Route
                    path="/login"
                    element={<Login />}
                />

                <Route
                    path="/register"
                    element={<Register />}
                />

                <Route
                    element={
                        <ProtectedRoute
                            allowedRoles={[
                                'Admin',
                                'ShopperAssistant',
                                'Customer',
                            ]}
                        />
                    }
                >
                    <Route
                        path="/"
                        element={<RoleRedirect />}
                    />
                </Route>

                <Route
                    element={
                        <ProtectedRoute
                            allowedRoles={['Admin']}
                        />
                    }
                >
                    <Route
                        path="/admin"
                        element={<AdminLayout />}
                    >
                        <Route
                            index
                            element={<Dashboard />}
                        />

                        <Route
                            path="customers"
                            element={<Customers />}
                        />

                        <Route
                            path="shopper-assistants"
                            element={
                                <ShopperAssistants />
                            }
                        />

                        <Route
                            path="products"
                            element={<Products />}
                        />

                        <Route
                            path="orders"
                            element={<AdminOrders />}
                        />

                        <Route
                            path="profile"
                            element={<AdminProfile />}
                        />
                    </Route>

                    <Route
                        element={
                            <ProtectedRoute
                                allowedRoles={['Admin']}
                                allowedAdminPositions={[
                                    'SuperAdmin',
                                ]}
                            />
                        }
                    >
                        <Route
                            path="/admin"
                            element={<AdminLayout />}
                        >
                            <Route
                                path="markets"
                                element={<Markets />}
                            />

                            <Route
                                path="categories"
                                element={<Categories />}
                            />

                            <Route
                                path="admins"
                                element={<Admins />}
                            />
                        </Route>
                    </Route>

                    <Route
                        element={
                            <ProtectedRoute
                                allowedRoles={['Admin']}
                                allowedAdminPositions={[
                                    'MarketAdmin',
                                ]}
                            />
                        }
                    >
                        <Route
                            path="/admin"
                            element={<AdminLayout />}
                        >
                            <Route
                                path="my-market"
                                element={<MyMarket />}
                            />
                        </Route>
                    </Route>
                </Route>

                <Route
                    element={
                        <ProtectedRoute
                            allowedRoles={[
                                'ShopperAssistant',
                            ]}
                        />
                    }
                >
                    <Route
                        path="/shopper-assistant"
                        element={
                            <ShopperAssistantLayout />
                        }
                    >
                        <Route
                            index
                            element={
                                <ShopperAssistantDashboard />
                            }
                        />

                        <Route
                            path="orders"
                            element={<Orders />}
                        />

                        <Route
                            path="profile"
                            element={
                                <ShopperAssistantProfile />
                            }
                        />
                    </Route>
                </Route>

                <Route
                    element={
                        <ProtectedRoute
                            allowedRoles={['Customer']}
                        />
                    }
                >
                    <Route
                        path="/customer"
                        element={<CustomerLayout />}
                    >
                        <Route
                            index
                            element={<CustomerDashboard />}
                        />

                        <Route
                            path="products"
                            element={<CustomerProducts />}
                        />

                        <Route
                            path="cart"
                            element={<CustomerCart />}
                        />

                        <Route
                            path="order-history"
                            element={<OrderHistory />}
                        />

                        <Route
                            path="profile"
                            element={<CustomerProfile />}
                        />
                    </Route>
                </Route>
            </Routes>
        </BrowserRouter>
    )
}

export default App