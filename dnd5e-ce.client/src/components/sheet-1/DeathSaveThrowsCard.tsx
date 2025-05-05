import Card from 'react-bootstrap/Card';
import Col from 'react-bootstrap/Col';
import Form from 'react-bootstrap/Form';

import HitDiceSVG from './assets/HitDice.svg';

import { useAppDispatch, useAppSelector } from '../../hooks';
import { selectDeathSaveThrowsFailures, selectDeathSaveThrowsSuccesses } from '../../store/selectors/sheet1Selectors';
import { updateDeathSaveThrowsFailures, updateDeathSaveThrowsSuccesses } from '../../store/sheet1Slice';

const DeathSaveThrowsCard: React.FC = () => {
  const dispatch = useAppDispatch();

  const successes = useAppSelector(selectDeathSaveThrowsSuccesses);
  const failures = useAppSelector(selectDeathSaveThrowsFailures);

  const handleSuccessChange = (index: number, checked: boolean) => {
    const newSuccesses = [...successes] as [boolean, boolean, boolean];
    // Устанавливаем значения до указанного индекса включительно
    for (let i = 0; i <= index; i++) {
      newSuccesses[i] = checked;
    }
    // Сбрасываем последующие значения
    for (let i = index + 1; i < newSuccesses.length; i++) {
      newSuccesses[i] = false;
    }
    dispatch(updateDeathSaveThrowsSuccesses(newSuccesses));
  };

  const handleFailureChange = (index: number, checked: boolean) => {
    const newFailures = [...failures] as [boolean, boolean, boolean];
    for (let i = 0; i <= index; i++) {
      newFailures[i] = checked;
    }
    for (let i = index + 1; i < newFailures.length; i++) {
      newFailures[i] = false;
    }
    dispatch(updateDeathSaveThrowsFailures(newFailures));
  };

  const renderCheckboxGroup = (
    title: string, groupId: string, group: boolean[],
    handleChange: (index: number, checked: boolean) => void
  ) => (
    <Card className="p-1 py-0 d-flex flex-row gap-0 border-0 bg-transparent">
      <Col md={6}>
        <Form.Label className="fw-lighter">
          {title}
        </Form.Label>
      </Col>
      <Col md={6}>
        {group.map((checked, index) => (
          <Form.Check
            key={`${groupId}-${index}`}
            inline
            id={`${groupId}-${index}`}
            className="p-0 m-0"
            checked={checked}
            onChange={(e) => handleChange(index, e.target.checked)}
            aria-label={`${title} ${index + 1}`}
          />
        ))}
      </Col>
    </Card>
  );


  return (
    <Card className="border-0 p-0 m-0 death-save-card">
      <Card.Img src={HitDiceSVG} alt="Фон для спасбросков от смерти" />
      <Card.ImgOverlay className="p-0 pt-0 px-2 m-0 d-flex flex-column">
        {renderCheckboxGroup('Успехи', 'characterDeathThrowSuccess', successes, handleSuccessChange)}
        {renderCheckboxGroup('Провалы', 'characterDeathThrowFailure', failures, handleFailureChange)}
        <div className="">
          <Form.Label className="p-0 m-0 text-center text-uppercase fw-bold" style={{ fontSize: "0.7rem" }}>
            Спасброски от смерти
          </Form.Label>
        </div>
      </Card.ImgOverlay>
    </Card>
  );
}

export default DeathSaveThrowsCard;