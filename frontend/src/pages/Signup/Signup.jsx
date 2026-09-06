import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import Input from "../../components/Input/Input";
import Button from "../../components/Button/Button";
import styles from "./Signup.module.css";

export default function Signup() {
  const { signup } = useAuth();
  const navigate = useNavigate();

  const [form, setForm] = useState({
    fullName: "",
    email: "",
    password: "",
    confirmPassword: "",
  });
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);

  function handleChange(e) {
    setForm({ ...form, [e.target.name]: e.target.value });
  }

  async function handleSubmit(e) {
    e.preventDefault();
    setError("");

    if (form.password !== form.confirmPassword) {
      setError("Passwords do not match.");
      return;
    }

    setLoading(true);
    try {
      await signup({
        fullName: form.fullName,
        email: form.email,
        password: form.password,
      });
      navigate("/login", { state: { justSignedUp: true } });
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }

  return (
    <div className={styles.wrapper}>
      <form className={styles.card} onSubmit={handleSubmit} style={{ animation: 'fadeIn 0.6s ease' }}>
        <h1 className={styles.title}>Sign Up</h1>
        <p className={styles.subtitle}>Create an account to get started</p>

        {error && <div className={styles.error}>{error}</div>}

        <Input
          id="fullName"
          name="fullName"
          type="text"
          label="Full Name"
          value={form.fullName}
          onChange={handleChange}
          required
        />
        <Input
          id="email"
          name="email"
          type="email"
          label="Email"
          value={form.email}
          onChange={handleChange}
          required
        />

        <div style={{ position: 'relative', marginBottom: '16px' }}>
          <Input
            id="password"
            name="password"
            type={showPassword ? "text" : "password"}
            label="Password"
            value={form.password}
            onChange={handleChange}
            required
          />
          <button
            type="button"
            onClick={() => setShowPassword(!showPassword)}
            style={{
              position: 'absolute', right: 12, top: '38px', transform: 'translateY(-50%)',
              background: 'none', border: 'none', cursor: 'pointer', color: '#5f6368',
              padding: 4, fontSize: '18px',
            }}
          >
            {showPassword ? '👁️' : '👁️‍🗨️'}
          </button>
        </div>
        <p className={styles.hint}>
          At least 8 characters, with uppercase, lowercase, and a special character.
        </p>

        <div style={{ position: 'relative', marginBottom: '16px' }}>
          <Input
            id="confirmPassword"
            name="confirmPassword"
            type={showConfirmPassword ? "text" : "password"}
            label="Confirm Password"
            value={form.confirmPassword}
            onChange={handleChange}
            required
          />
          <button
            type="button"
            onClick={() => setShowConfirmPassword(!showConfirmPassword)}
            style={{
              position: 'absolute', right: 12, top: '38px', transform: 'translateY(-50%)',
              background: 'none', border: 'none', cursor: 'pointer', color: '#5f6368',
              padding: 4, fontSize: '18px',
            }}
          >
            {showConfirmPassword ? '👁️' : '👁️‍🗨️'}
          </button>
        </div>

        <Button type="submit" disabled={loading} fullWidth>
          {loading ? (
            <span style={{ display: 'flex', alignItems: 'center', justifyContent: 'center', gap: '8px' }}>
              <span style={{ animation: 'spin 1s linear infinite' }}>⚙️</span>
              Creating account...
            </span>
          ) : (
            "Sign Up"
          )}
        </Button>

        <p className={styles.footer}>
          Already have an account? <Link to="/login">Log in</Link>
        </p>
      </form>

      <div className={styles.infoPanel}>
        <h2 className={styles.infoTitle}>Task Management Tool</h2>
        <p className={styles.infoText}>
          Create, assign, and track tasks across your team in one place.
        </p>
        <ul className={styles.featureList}>
          <li>Track task status, priority, and category</li>
          <li>Admins can assign tasks to any team member</li>
          <li>Dashboard overview of To Do, In Progress, and Completed work</li>
          <li>Role-based access keeps personal tasks private</li>
        </ul>
      </div>
    </div>
  );
}