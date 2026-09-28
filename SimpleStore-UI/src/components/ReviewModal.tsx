import { useEffect, useState } from 'react'
import {
    Button,
    Input,
    Modal,
    Rate,
    Typography,
    message,
} from 'antd'
import { useMutation, useQueryClient } from '@tanstack/react-query'

import { submitReview } from '../services/reviews/reviewService'

const { TextArea } = Input
const { Text } = Typography

interface ReviewModalProps {
    open: boolean
    orderId: number
    productId: number
    productName: string
    onClose: () => void
}

function ReviewModal({
    open,
    orderId,
    productId,
    productName,
    onClose,
}: ReviewModalProps) {
    const [rating, setRating] = useState<number>(5)
    const [comment, setComment] = useState<string>('')

    const queryClient = useQueryClient()

    useEffect(() => {
        if (open) {
            setRating(5)
            setComment('')
        }
    }, [open])

    const submitMutation = useMutation({
        mutationFn: () =>
            submitReview({
                orderId,
                productId,
                rating,
                message: comment.trim() || undefined,
            }),
        onSuccess: () => {
            message.success('Review submitted')
            queryClient.invalidateQueries({
                queryKey: ['reviews-by-product', productId],
            })
            onClose()
        },
        onError: (error) => {
            message.error(
                error instanceof Error
                    ? error.message
                    : 'Failed to submit review.'
            )
        },
    })

    const handleSubmit = () => {
        if (rating < 1 || rating > 5) {
            message.warning('Please select a rating from 1 to 5.')
            return
        }
        submitMutation.mutate()
    }

    return (
        <Modal
            open={open}
            title="Leave a Review"
            onCancel={onClose}
            footer={null}
            destroyOnClose
        >
            <div className="space-y-5 pt-2">
                <div>
                    <Text className="text-[#6b7280] text-sm">
                        Product
                    </Text>
                    <div className="text-[#0a0a0a] font-medium mt-1">
                        {productName}
                    </div>
                </div>

                <div>
                    <Text className="text-[#6b7280] text-sm block mb-2">
                        Rating
                    </Text>
                    <Rate
                        value={rating}
                        onChange={setRating}
                        disabled={submitMutation.isPending}
                    />
                </div>

                <div>
                    <Text className="text-[#6b7280] text-sm block mb-2">
                        Comment (optional)
                    </Text>
                    <TextArea
                        rows={4}
                        value={comment}
                        onChange={(event) =>
                            setComment(event.target.value)
                        }
                        maxLength={500}
                        showCount
                        disabled={submitMutation.isPending}
                        placeholder="Share your thoughts about this product…"
                    />
                </div>

                <div className="flex justify-end gap-2 pt-2">
                    <Button
                        onClick={onClose}
                        disabled={submitMutation.isPending}
                    >
                        Cancel
                    </Button>
                    <Button
                        type="primary"
                        onClick={handleSubmit}
                        loading={submitMutation.isPending}
                    >
                        Submit Review
                    </Button>
                </div>
            </div>
        </Modal>
    )
}

export default ReviewModal
