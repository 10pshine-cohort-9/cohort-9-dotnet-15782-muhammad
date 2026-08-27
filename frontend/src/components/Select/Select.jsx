import { useState, useRef, useEffect } from "react";
import { ChevronDown, Check } from "lucide-react";
import styles from "./Select.module.css";

export default function Select({ value, onChange, options, placeholder = "Select..." }) {
  const [open, setOpen] = useState(false);
  const ref = useRef(null);

  useEffect(() => {
    function handleClickOutside(e) {
      if (ref.current && !ref.current.contains(e.target)) setOpen(false);
    }
    function handleEscape(e) {
      if (e.key === "Escape") setOpen(false);
    }
    document.addEventListener("mousedown", handleClickOutside);
    document.addEventListener("keydown", handleEscape);
    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
      document.removeEventListener("keydown", handleEscape);
    };
  }, []);

  const selectedLabel = options.find((o) => String(o.value) === String(value))?.label;

  return (
    <div className={styles.wrapper} ref={ref}>
      <button type="button" className={styles.trigger} onClick={() => setOpen((o) => !o)}>
        <span className={value ? styles.value : styles.placeholder}>
          {selectedLabel || placeholder}
        </span>
        <ChevronDown size={18} className={`${styles.chevron} ${open ? styles.chevronOpen : ""}`} />
      </button>

      {open && (
        <ul className={styles.menu}>
          <li className={styles.option} onClick={() => { onChange(""); setOpen(false); }}>
            {placeholder}
          </li>
          {options.map((opt) => (
            <li
              key={opt.value}
              className={`${styles.option} ${String(opt.value) === String(value) ? styles.optionSelected : ""}`}
              onClick={() => { onChange(opt.value); setOpen(false); }}
            >
              <span>{opt.label}</span>
              {String(opt.value) === String(value) && <Check size={16} />}
            </li>
          ))}
        </ul>
      )}
    </div>
  );
}