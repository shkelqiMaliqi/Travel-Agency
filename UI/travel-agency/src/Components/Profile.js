import React, { useState } from 'react';
import { Link, useNavigate } from 'react-router-dom';

const Profile = () => {
  const [isLogin, setIsLogin] = useState(true);
  const navigate = useNavigate();

  const handleToggle = () => {
    if (isLogin) {
      navigate('/registerpage');
    } else {
      navigate('/loginpage');
    }
  };

  return (
    <div className="container">
      <div className="card mx-auto mt-5 p-4" style={{ maxWidth: '400px' }}>
        <h2 className="text-center">{isLogin ? 'Login' : 'Register'}</h2>
        <form>
          {!isLogin && (
            <>
              <div className="mb-3">
                <label htmlFor="username" className="form-label">
                  Username
                </label>
                <input
                  type="text"
                  id="username"
                  className="form-control"
                  placeholder="Enter your username"
                  required
                />
              </div>
            </>
          )}
          <div className="mb-3">
            <label htmlFor="email" className="form-label">
              Email
            </label>
            <input
              type="email"
              id="email"
              className="form-control"
              placeholder="Enter your email"
              required
            />
          </div>
          <div className="mb-3">
            <label htmlFor="password" className="form-label">
              Password
            </label>
            <input
              type="password"
              id="password"
              className="form-control"
              placeholder="Enter your password"
              required
            />
          </div>
          <button type="button" className="btn btn-primary w-100" onClick={handleToggle}>
            {isLogin ? 'Login' : 'Register'}
          </button>
        </form>
        <p className="text-center mt-3">
          {isLogin ? (
            <>
              Don't have an account?{' '}
              <Link to="/RegisterPage">Register here</Link>
            </>
          ) : (
            <>
              Already have an account?{' '}
              <Link to="/loginpage">Login here</Link>
            </>
          )}
        </p>
      </div>
    </div>
  );
};

export default Profile;
