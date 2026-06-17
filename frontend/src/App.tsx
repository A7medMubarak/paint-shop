import { Routes, Route } from 'react-router-dom';
import { Layout } from './components/Layout';
import LoginPage from './pages/LoginPage';
import DashboardPage from './pages/DashboardPage';
import ProductsPage from './pages/ProductsPage';
import InventoryPage from './pages/InventoryPage';
import MovementsPage from './pages/MovementsPage';
import SalesPage from './pages/SalesPage';
import CreateSalePage from './pages/CreateSalePage';
import SaleDetailPage from './pages/SaleDetailPage';
import InvoicePage from './pages/InvoicePage';
import CustomersPage from './pages/CustomersPage';
import CustomerDetailPage from './pages/CustomerDetailPage';
import DailyReportPage from './pages/DailyReportPage';
import LowStockPage from './pages/LowStockPage';
import TopSellingPage from './pages/TopSellingPage';
import PeriodReportPage from './pages/PeriodReportPage';
import InventoryValuationPage from './pages/InventoryValuationPage';
import UsersPage from './pages/UsersPage';

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<LoginPage />} />
      <Route element={<Layout />}>
        <Route index element={<DashboardPage />} />
        <Route path="products" element={<ProductsPage />} />
        <Route path="inventory" element={<InventoryPage />} />
        <Route path="inventory/movements/:variantId" element={<MovementsPage />} />
        <Route path="sales" element={<SalesPage />} />
        <Route path="sales/new" element={<CreateSalePage />} />
        <Route path="sales/:id" element={<SaleDetailPage />} />
        <Route path="sales/:id/invoice" element={<InvoicePage />} />
        <Route path="customers" element={<CustomersPage />} />
        <Route path="customers/:id" element={<CustomerDetailPage />} />
        <Route path="reports/daily" element={<DailyReportPage />} />
        <Route path="reports/low-stock" element={<LowStockPage />} />
        <Route path="reports/top-selling" element={<TopSellingPage />} />
        <Route path="reports/period" element={<PeriodReportPage />} />
        <Route path="reports/inventory-valuation" element={<InventoryValuationPage />} />
        <Route path="users" element={<UsersPage />} />
      </Route>
    </Routes>
  );
}
